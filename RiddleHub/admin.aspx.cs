using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web;

namespace RiddleHub
{
    public partial class admin : System.Web.UI.Page
    {
        public string StatusHtml = string.Empty;
        public string SummaryHtml = string.Empty;
        public string UsersTableHtml = string.Empty;
        public string RiddlesTableHtml = string.Empty;
        public string SqlResultHtml = string.Empty;
        public string SqlMode = "query";
        public string EncodedSqlInput = string.Empty;
        public bool IsAdminAuthenticated;
        public bool IsAdminConfigured;

        private const string AdminPasswordKey = "RIDDLEHUB_ADMIN_PASSWORD";
        private const string AdminSessionKey = "RIDDLEHUB_ADMIN_AUTH";
        private string _configuredAdminPassword = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            string action = (Request.Form["action"] ?? string.Empty).Trim().ToLowerInvariant();

            if (action == "admin_logout")
            {
                Session.Remove(AdminSessionKey);
                SetStatus("Logged out.");
            }

            _configuredAdminPassword = ResolveAdminPassword();
            IsAdminConfigured = !string.IsNullOrWhiteSpace(_configuredAdminPassword);
            IsAdminAuthenticated = IsAdminSessionAuthenticated();

            if (!IsAdminConfigured)
            {
                SetStatus("Admin password is not configured. Set RIDDLEHUB_ADMIN_PASSWORD in environment or .env.", true);
                return;
            }

            if (Request.HttpMethod == "POST" && action == "admin_login")
            {
                HandleAdminLogin();
                IsAdminAuthenticated = IsAdminSessionAuthenticated();
            }

            if (!IsAdminAuthenticated)
            {
                return;
            }

            if (Request.HttpMethod == "POST" && action != "admin_login" && action != "admin_logout")
            {
                HandlePostAction(action);
            }

            LoadDashboardData();
        }

        private bool IsAdminSessionAuthenticated()
        {
            object value = Session[AdminSessionKey];
            if (value is bool boolValue)
            {
                return boolValue;
            }

            bool parsed;
            return value != null && bool.TryParse(Convert.ToString(value), out parsed) && parsed;
        }

        private void HandleAdminLogin()
        {
            string providedPassword = (Request.Form["admin_password"] ?? string.Empty).Trim();
            if (providedPassword == _configuredAdminPassword)
            {
                Session[AdminSessionKey] = true;
                SetStatus("Admin login successful.");
                return;
            }

            Session.Remove(AdminSessionKey);
            SetStatus("Invalid admin password.", true);
        }

        private string ResolveAdminPassword()
        {
            string directEnv = Environment.GetEnvironmentVariable(AdminPasswordKey);
            if (!string.IsNullOrWhiteSpace(directEnv))
            {
                return directEnv.Trim();
            }

            foreach (string envPath in GetDotEnvCandidatePaths())
            {
                string value = ReadEnvValueFromFile(envPath, AdminPasswordKey);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return string.Empty;
        }

        private IEnumerable<string> GetDotEnvCandidatePaths()
        {
            string appRoot = Server.MapPath("~/");
            var candidates = new List<string>();

            if (!string.IsNullOrWhiteSpace(appRoot))
            {
                candidates.Add(Path.Combine(appRoot, ".env"));

                string parent = Path.GetFullPath(Path.Combine(appRoot, ".."));
                candidates.Add(Path.Combine(parent, ".env"));
            }

            string processCwd = Directory.GetCurrentDirectory();
            if (!string.IsNullOrWhiteSpace(processCwd))
            {
                candidates.Add(Path.Combine(processCwd, ".env"));
            }

            var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string path in candidates)
            {
                if (!string.IsNullOrWhiteSpace(path) && unique.Add(path))
                {
                    yield return path;
                }
            }
        }

        private static string ReadEnvValueFromFile(string path, string key)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return null;
            }

            foreach (string rawLine in File.ReadAllLines(path))
            {
                string line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith("#"))
                {
                    continue;
                }

                int separatorIndex = line.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                string parsedKey = line.Substring(0, separatorIndex).Trim();
                if (!string.Equals(parsedKey, key, StringComparison.Ordinal))
                {
                    continue;
                }

                string value = line.Substring(separatorIndex + 1).Trim();
                if ((value.StartsWith("\"") && value.EndsWith("\"")) || (value.StartsWith("'") && value.EndsWith("'")))
                {
                    value = value.Substring(1, value.Length - 2);
                }
                return value;
            }

            return null;
        }

        private void HandlePostAction(string action)
        {
            try
            {
                switch (action)
                {
                    case "refresh":
                        SetStatus("Refreshed.");
                        break;
                    case "seed_demo":
                        SeedDemoData();
                        SetStatus("Demo data ensured.");
                        break;
                    case "clear_riddles":
                        ClearRiddles();
                        SetStatus("All riddles deleted.");
                        break;
                    case "clear_all":
                        ResetUsersAndRiddles();
                        SetStatus("Users+riddles reset to demo state.");
                        break;
                    case "delete_user":
                        DeleteUserByUsername(Request.Form["username"]);
                        break;
                    case "delete_riddle":
                        DeleteRiddleById(Request.Form["riddle_id"]);
                        break;
                    case "run_sql":
                        RunSqlFromForm();
                        break;
                    default:
                        SetStatus("Unknown action: " + Encode(action), true);
                        break;
                }
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }

        private void LoadDashboardData()
        {
            using (SqlConnection conn = Helper.ConnectToDb("db.mdf"))
            {
                conn.Open();

                DataTable summary = QueryDataTable(conn,
                    "SELECT (SELECT COUNT(*) FROM dbo.[user]) AS users_count, (SELECT COUNT(*) FROM dbo.[riddle]) AS riddles_count;");
                SummaryHtml = RenderSummary(summary);

                DataTable users = QueryDataTable(conn, @"
SELECT
    u.username,
    u.email,
    u.[password],
    (SELECT COUNT(*) FROM dbo.[riddle] r WHERE r.username = u.username) AS riddle_count
FROM dbo.[user] u
ORDER BY u.username;");
                UsersTableHtml = RenderDataTable(users, "No users found.");

                DataTable riddles = QueryDataTable(conn, @"
SELECT
    r.riddle_id,
    r.username,
    r.riddle_text,
    r.riddle_hint,
    r.answer
FROM dbo.[riddle] r
ORDER BY r.riddle_id DESC;");
                RiddlesTableHtml = RenderDataTable(riddles, "No riddles found.");
            }
        }

        private void SeedDemoData()
        {
            const string sql = @"
IF NOT EXISTS (SELECT 1 FROM dbo.[user] WHERE [username] = N'demo')
BEGIN
    INSERT INTO dbo.[user] ([username], [password], [email])
    VALUES (N'demo', N'demo', N'demo@local');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.[riddle] WHERE [username] = N'demo')
BEGIN
    INSERT INTO dbo.[riddle] ([riddle_text], [riddle_hint], [answer], [username]) VALUES
    (N'What has keys but can''t open locks?', N'Music', N'A piano', N'demo'),
    (N'I speak without a mouth and hear without ears. What am I?', N'Sound', N'An echo', N'demo'),
    (N'What can travel around the world while staying in one corner?', N'Letters', N'A stamp', N'demo');
END;";

            ExecuteNonQuery(sql);
        }

        private void ClearRiddles()
        {
            ExecuteNonQuery("DELETE FROM dbo.[riddle];");
        }

        private void ResetUsersAndRiddles()
        {
            const string sql = @"
DELETE FROM dbo.[riddle];
DELETE FROM dbo.[user];

INSERT INTO dbo.[user] ([username], [password], [email])
VALUES (N'demo', N'demo', N'demo@local');

INSERT INTO dbo.[riddle] ([riddle_text], [riddle_hint], [answer], [username]) VALUES
(N'What has keys but can''t open locks?', N'Music', N'A piano', N'demo'),
(N'I speak without a mouth and hear without ears. What am I?', N'Sound', N'An echo', N'demo'),
(N'What can travel around the world while staying in one corner?', N'Letters', N'A stamp', N'demo');";

            ExecuteNonQuery(sql);
        }

        private void DeleteUserByUsername(string username)
        {
            string cleaned = (username ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(cleaned))
            {
                SetStatus("Username is required.", true);
                return;
            }

            using (SqlConnection conn = Helper.ConnectToDb("db.mdf"))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "DELETE FROM dbo.[riddle] WHERE username = @Username; DELETE FROM dbo.[user] WHERE username = @Username;",
                    conn))
                {
                    cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 300).Value = cleaned;
                    cmd.ExecuteNonQuery();
                }
            }

            SetStatus("Deleted user (and related riddles): " + Encode(cleaned));
        }

        private void DeleteRiddleById(string riddleIdText)
        {
            int riddleId;
            if (!int.TryParse((riddleIdText ?? string.Empty).Trim(), out riddleId) || riddleId <= 0)
            {
                SetStatus("Valid riddle_id is required.", true);
                return;
            }

            using (SqlConnection conn = Helper.ConnectToDb("db.mdf"))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("DELETE FROM dbo.[riddle] WHERE riddle_id = @RiddleId;", conn))
                {
                    cmd.Parameters.Add("@RiddleId", SqlDbType.Int).Value = riddleId;
                    int rows = cmd.ExecuteNonQuery();
                    SetStatus("Deleted riddles: " + rows);
                }
            }
        }

        private void RunSqlFromForm()
        {
            string sql = Request.Form["sql"] ?? string.Empty;
            SqlMode = ((Request.Form["sql_mode"] ?? "query").Trim().ToLowerInvariant() == "exec") ? "exec" : "query";
            EncodedSqlInput = Encode(sql);

            if (string.IsNullOrWhiteSpace(sql))
            {
                SetStatus("SQL input is empty.", true);
                return;
            }

            using (SqlConnection conn = Helper.ConnectToDb("db.mdf"))
            {
                conn.Open();
                if (SqlMode == "exec")
                {
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        int rows = cmd.ExecuteNonQuery();
                        SetStatus("SQL executed. Rows affected: " + rows);
                    }
                    SqlResultHtml = string.Empty;
                }
                else
                {
                    DataTable dt = new DataTable();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                    SqlResultHtml = RenderDataTable(dt, "Query returned 0 rows.");
                    SetStatus("Query executed. Rows returned: " + dt.Rows.Count);
                }
            }
        }

        private static DataTable QueryDataTable(SqlConnection conn, string sql)
        {
            DataTable dt = new DataTable();
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                adapter.Fill(dt);
            }
            return dt;
        }

        private void ExecuteNonQuery(string sql)
        {
            using (SqlConnection conn = Helper.ConnectToDb("db.mdf"))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void SetStatus(string message, bool isError = false)
        {
            if (isError)
            {
                StatusHtml = "<div class=\"status\" style=\"background:#fef2f2;border-color:#fecaca;color:#7f1d1d;\">" + Encode(message) + "</div>";
            }
            else
            {
                StatusHtml = "<div class=\"status\">" + Encode(message) + "</div>";
            }
        }

        private static string RenderSummary(DataTable dt)
        {
            if (dt.Rows.Count == 0)
            {
                return "<p class='muted'>No summary available.</p>";
            }

            DataRow row = dt.Rows[0];
            StringBuilder sb = new StringBuilder();
            sb.Append("<div class='summary-grid'>");
            sb.Append("<div class='summary-box'><div class='label'>Users</div><div class='value'>");
            sb.Append(Encode(row["users_count"]));
            sb.Append("</div></div>");
            sb.Append("<div class='summary-box'><div class='label'>Riddles</div><div class='value'>");
            sb.Append(Encode(row["riddles_count"]));
            sb.Append("</div></div>");
            sb.Append("</div>");
            return sb.ToString();
        }

        private static string RenderDataTable(DataTable dt, string emptyMessage)
        {
            if (dt.Rows.Count == 0)
            {
                return "<p class='muted'>" + Encode(emptyMessage) + "</p>";
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("<div class='table-wrap'><table><thead><tr>");
            foreach (DataColumn col in dt.Columns)
            {
                sb.Append("<th>").Append(Encode(col.ColumnName)).Append("</th>");
            }
            sb.Append("</tr></thead><tbody>");

            foreach (DataRow row in dt.Rows)
            {
                sb.Append("<tr>");
                foreach (DataColumn col in dt.Columns)
                {
                    sb.Append("<td>").Append(Encode(row[col])).Append("</td>");
                }
                sb.Append("</tr>");
            }

            sb.Append("</tbody></table></div>");
            return sb.ToString();
        }

        private static string Encode(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }
            return HttpUtility.HtmlEncode(Convert.ToString(value));
        }
    }
}
