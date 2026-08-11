using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using RiddleHub.Security;

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
        public string CsrfTokenEncoded = string.Empty;
        public bool IsAdminAuthenticated;
        public bool IsAdminConfigured;

        private const string AdminPasswordKey = "RIDDLEHUB_ADMIN_PASSWORD";
        private const string AdminEnvFileKey = "RIDDLEHUB_ENV_FILE";
        private const string AdminSessionKey = "RIDDLEHUB_ADMIN_AUTH";
        private string _configuredAdminPassword = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            CsrfTokenEncoded = CsrfProtection.GetEncodedToken(Session);
            string action = (Request.Form["action"] ?? string.Empty).Trim().ToLowerInvariant();
            if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) &&
                CsrfProtection.RejectInvalidPost(Request, Response, Session))
            {
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            _configuredAdminPassword = ResolveAdminPassword();
            IsAdminConfigured = !string.IsNullOrWhiteSpace(_configuredAdminPassword) &&
                _configuredAdminPassword.Length >= 16 && _configuredAdminPassword.Length <= 256;
            IsAdminAuthenticated = IsAdminSessionAuthenticated();
            if (!IsAdminConfigured)
            {
                Session.Remove(AdminSessionKey);
                SetStatus("Admin password must be configured as a unique 16-256 character RIDDLEHUB_ADMIN_PASSWORD in the process environment.", true);
                return;
            }

            if (Request.HttpMethod == "POST" && action == "admin_logout")
            {
                Session.Remove(AdminSessionKey);
                IsAdminAuthenticated = false;
                SetStatus("Logged out.");
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
            if (Request.HttpMethod == "POST" && action != "admin_login")
            {
                HandlePostAction(action);
            }
            LoadDashboardData();
        }

        private bool IsAdminSessionAuthenticated()
        {
            string sessionFingerprint = Convert.ToString(Session[AdminSessionKey]);
            return !string.IsNullOrEmpty(_configuredAdminPassword) &&
                !string.IsNullOrEmpty(sessionFingerprint) &&
                PasswordSecurity.FixedTimeEqualsUtf8(
                    sessionFingerprint,
                    CreatePasswordFingerprint(_configuredAdminPassword));
        }

        private void HandleAdminLogin()
        {
            string providedPassword = (Request.Form["admin_password"] ?? string.Empty).Trim();
            int retryAfterSeconds;
            if (!AuthRateLimiter.IsAllowed(Request, "admin-login", "admin", out retryAfterSeconds))
            {
                Response.StatusCode = 429;
                Response.AddHeader("Retry-After", retryAfterSeconds.ToString());
                SetStatus("Too many admin login attempts. Try again later.", true);
                return;
            }
            if (providedPassword.Length == 0 || providedPassword.Length > 256)
            {
                AuthRateLimiter.RecordFailure(Request, "admin-login", "admin");
                Session.Remove(AdminSessionKey);
                SetStatus("Invalid admin password.", true);
                return;
            }

            if (PasswordSecurity.FixedTimeEqualsUtf8(providedPassword, _configuredAdminPassword))
            {
                AuthRateLimiter.Reset(Request, "admin-login", "admin");
                Session[AdminSessionKey] = CreatePasswordFingerprint(_configuredAdminPassword);
                SetStatus("Admin login successful.");
                return;
            }

            AuthRateLimiter.RecordFailure(Request, "admin-login", "admin");
            Session.Remove(AdminSessionKey);
            SetStatus("Invalid admin password.", true);
        }

        private static string CreatePasswordFingerprint(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password ?? string.Empty)));
            }
        }

        private string ResolveAdminPassword()
        {
            string directEnvironment = Environment.GetEnvironmentVariable(AdminPasswordKey);
            if (!string.IsNullOrWhiteSpace(directEnvironment))
            {
                return directEnvironment.Trim();
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
            string appRoot = Path.GetFullPath(Server.MapPath("~/"));
            var candidates = new List<string>();
            string explicitFile = Environment.GetEnvironmentVariable(AdminEnvFileKey);
            if (!string.IsNullOrWhiteSpace(explicitFile))
            {
                candidates.Add(Path.GetFullPath(explicitFile));
            }
            candidates.Add(Path.GetFullPath(Path.Combine(appRoot, "..", ".env")));

            string processDirectory = Directory.GetCurrentDirectory();
            if (!string.IsNullOrWhiteSpace(processDirectory))
            {
                candidates.Add(Path.GetFullPath(Path.Combine(processDirectory, ".env")));
            }

            var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string path in candidates)
            {
                if (IsOutsideDirectory(path, appRoot) && unique.Add(path))
                {
                    yield return path;
                }
            }
        }

        private static bool IsOutsideDirectory(string path, string directory)
        {
            string normalizedPath = Path.GetFullPath(path);
            string normalizedDirectory = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar) +
                Path.DirectorySeparatorChar;
            return !normalizedPath.StartsWith(normalizedDirectory, StringComparison.OrdinalIgnoreCase);
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
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }
                int separatorIndex = line.IndexOf('=');
                if (separatorIndex <= 0 ||
                    !string.Equals(line.Substring(0, separatorIndex).Trim(), key, StringComparison.Ordinal))
                {
                    continue;
                }
                string value = line.Substring(separatorIndex + 1).Trim();
                if (value.Length >= 2 &&
                    ((value.StartsWith("\"", StringComparison.Ordinal) && value.EndsWith("\"", StringComparison.Ordinal)) ||
                     (value.StartsWith("'", StringComparison.Ordinal) && value.EndsWith("'", StringComparison.Ordinal))))
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
                        SetStatus("Demo data ensured. The demo account has no usable password.");
                        break;
                    case "clear_riddles":
                        ExecuteNonQuery("DELETE FROM dbo.[riddle];");
                        SetStatus("All riddles deleted.");
                        break;
                    case "clear_all":
                        ResetUsersAndRiddles();
                        SetStatus("Users and riddles reset to a disabled demo account.");
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
                        SetStatus("Unknown action.", true);
                        break;
                }
            }
            catch (Exception exception)
            {
                SetStatus(exception.Message, true);
            }
        }

        private void LoadDashboardData()
        {
            using (SqlConnection connection = Helper.ConnectToDb())
            {
                connection.Open();
                SummaryHtml = RenderSummary(QueryDataTable(
                    connection,
                    "SELECT (SELECT COUNT(*) FROM dbo.[user]) AS users_count, (SELECT COUNT(*) FROM dbo.[riddle]) AS riddles_count;"));
                UsersTableHtml = RenderDataTable(QueryDataTable(connection, @"
SELECT
    u.[username],
    u.[email],
    u.[password_reset_required],
    u.[session_version],
    (SELECT COUNT(*) FROM dbo.[riddle] r WHERE r.[username] = u.[username]) AS riddle_count
FROM dbo.[user] u
ORDER BY u.[username];"), "No users found.");
                RiddlesTableHtml = RenderDataTable(QueryDataTable(connection, @"
SELECT [riddle_id], [username], [riddle_text], [riddle_hint], [answer]
FROM dbo.[riddle]
ORDER BY [riddle_id] DESC;"), "No riddles found.");
            }
        }

        private void SeedDemoData()
        {
            using (SqlConnection connection = Helper.ConnectToDb())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(@"
IF NOT EXISTS (SELECT 1 FROM dbo.[user] WHERE [username] = N'demo')
BEGIN
    INSERT INTO dbo.[user]
        ([username], [password], [email], [password_reset_required], [session_version])
    VALUES
        (N'demo', @UnusablePassword, N'demo@example.invalid', 1, 1);
END;
IF NOT EXISTS (SELECT 1 FROM dbo.[riddle] WHERE [username] = N'demo')
BEGIN
    INSERT INTO dbo.[riddle] ([riddle_text], [riddle_hint], [answer], [username]) VALUES
    (N'What has keys but can''t open locks?', N'Music', N'A piano', N'demo'),
    (N'I speak without a mouth and hear without ears. What am I?', N'Sound', N'An echo', N'demo'),
    (N'What can travel around the world while staying in one corner?', N'Letters', N'A stamp', N'demo');
END;", connection))
                {
                    command.Parameters.Add("@UnusablePassword", SqlDbType.NVarChar, 300).Value =
                        PasswordSecurity.CreateUnusablePasswordHash();
                    command.ExecuteNonQuery();
                }
            }
        }

        private void ResetUsersAndRiddles()
        {
            using (SqlConnection connection = Helper.ConnectToDb())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(@"
DELETE FROM dbo.[riddle];
DELETE FROM dbo.[user];
INSERT INTO dbo.[user]
    ([username], [password], [email], [password_reset_required], [session_version])
VALUES
    (N'demo', @UnusablePassword, N'demo@example.invalid', 1, 1);
INSERT INTO dbo.[riddle] ([riddle_text], [riddle_hint], [answer], [username]) VALUES
(N'What has keys but can''t open locks?', N'Music', N'A piano', N'demo'),
(N'I speak without a mouth and hear without ears. What am I?', N'Sound', N'An echo', N'demo'),
(N'What can travel around the world while staying in one corner?', N'Letters', N'A stamp', N'demo');", connection))
                {
                    command.Parameters.Add("@UnusablePassword", SqlDbType.NVarChar, 300).Value =
                        PasswordSecurity.CreateUnusablePasswordHash();
                    command.ExecuteNonQuery();
                }
            }
        }

        private void DeleteUserByUsername(string username)
        {
            string cleaned = (username ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(cleaned))
            {
                SetStatus("Username is required.", true);
                return;
            }
            using (SqlConnection connection = Helper.ConnectToDb())
            using (SqlCommand command = new SqlCommand(
                "DELETE FROM dbo.[riddle] WHERE [username] = @Username; DELETE FROM dbo.[user] WHERE [username] = @Username;",
                connection))
            {
                command.Parameters.Add("@Username", SqlDbType.NVarChar, 300).Value = cleaned;
                connection.Open();
                command.ExecuteNonQuery();
            }
            SetStatus("Deleted user and related riddles: " + cleaned);
        }

        private void DeleteRiddleById(string riddleIdText)
        {
            int riddleId;
            if (!int.TryParse((riddleIdText ?? string.Empty).Trim(), out riddleId) || riddleId <= 0)
            {
                SetStatus("Valid riddle_id is required.", true);
                return;
            }
            using (SqlConnection connection = Helper.ConnectToDb())
            using (SqlCommand command = new SqlCommand(
                "DELETE FROM dbo.[riddle] WHERE [riddle_id] = @RiddleId;",
                connection))
            {
                command.Parameters.Add("@RiddleId", SqlDbType.Int).Value = riddleId;
                connection.Open();
                SetStatus("Deleted riddles: " + command.ExecuteNonQuery());
            }
        }

        private void RunSqlFromForm()
        {
            string sql = Request.Form["sql"] ?? string.Empty;
            SqlMode = string.Equals(
                (Request.Form["sql_mode"] ?? "query").Trim(),
                "exec",
                StringComparison.OrdinalIgnoreCase) ? "exec" : "query";
            EncodedSqlInput = Encode(sql);
            if (string.IsNullOrWhiteSpace(sql) || sql.Length > 20000)
            {
                SetStatus("SQL input is empty or too long.", true);
                return;
            }

            using (SqlConnection connection = Helper.ConnectToDb())
            {
                connection.Open();
                if (SqlMode == "exec")
                {
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.CommandTimeout = 30;
                        SetStatus("SQL executed. Rows affected: " + command.ExecuteNonQuery());
                    }
                    SqlResultHtml = string.Empty;
                }
                else
                {
                    DataTable table = new DataTable();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        command.CommandTimeout = 30;
                        adapter.Fill(table);
                    }
                    SqlResultHtml = RenderDataTable(table, "Query returned 0 rows.");
                    SetStatus("Query executed. Rows returned: " + table.Rows.Count);
                }
            }
        }

        private static DataTable QueryDataTable(SqlConnection connection, string sql)
        {
            DataTable table = new DataTable();
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                adapter.Fill(table);
            }
            return table;
        }

        private static void ExecuteNonQuery(string sql)
        {
            using (SqlConnection connection = Helper.ConnectToDb())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private void SetStatus(string message, bool isError = false)
        {
            string style = isError
                ? " style=\"background:#fef2f2;border-color:#fecaca;color:#7f1d1d;\""
                : string.Empty;
            StatusHtml = "<div class=\"status\"" + style + ">" + Encode(message) + "</div>";
        }

        private static string RenderSummary(DataTable table)
        {
            if (table.Rows.Count == 0)
            {
                return "<p class='muted'>No summary available.</p>";
            }
            DataRow row = table.Rows[0];
            return "<div class='summary-grid'>" +
                "<div class='summary-box'><div class='label'>Users</div><div class='value'>" +
                Encode(row["users_count"]) + "</div></div>" +
                "<div class='summary-box'><div class='label'>Riddles</div><div class='value'>" +
                Encode(row["riddles_count"]) + "</div></div></div>";
        }

        private static string RenderDataTable(DataTable table, string emptyMessage)
        {
            if (table.Rows.Count == 0)
            {
                return "<p class='muted'>" + Encode(emptyMessage) + "</p>";
            }
            StringBuilder builder = new StringBuilder();
            builder.Append("<div class='table-wrap'><table><thead><tr>");
            foreach (DataColumn column in table.Columns)
            {
                builder.Append("<th>").Append(Encode(column.ColumnName)).Append("</th>");
            }
            builder.Append("</tr></thead><tbody>");
            foreach (DataRow row in table.Rows)
            {
                builder.Append("<tr>");
                foreach (DataColumn column in table.Columns)
                {
                    builder.Append("<td>").Append(Encode(row[column])).Append("</td>");
                }
                builder.Append("</tr>");
            }
            builder.Append("</tbody></table></div>");
            return builder.ToString();
        }

        private static string Encode(object value)
        {
            return value == null || value == DBNull.Value
                ? string.Empty
                : HttpUtility.HtmlEncode(Convert.ToString(value));
        }
    }
}
