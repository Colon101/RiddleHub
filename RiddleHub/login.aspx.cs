using System;
using System.Data.SqlClient;
using RiddleHub.Security;
using UtilFunctions;

namespace RiddleHub
{
    public partial class Login : System.Web.UI.Page
    {
        public string st = string.Empty;
        public string ReturnPage = "my";
        public bool LoginSucceeded;
        public bool PasswordResetRequired;
        public string LoggedInUsername = string.Empty;
        public string CsrfTokenEncoded = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            ReturnPage = SafeReturnPage(Request.Params["return"]);
            CsrfTokenEncoded = CsrfProtection.GetEncodedToken(Session);
            bool requiredLoginByQuery = !string.IsNullOrWhiteSpace(Request.QueryString["required"]);
            bool requiredLoginBySession = (Session["login_required"] as bool? ?? false) == true;

            if (Request.Form["submit"] != null)
            {
                if (CsrfProtection.RejectInvalidPost(Request, Response, Session))
                {
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }
                HandleLogin();
            }
            else if (requiredLoginByQuery || requiredLoginBySession)
            {
                st = "<p>You must log in first or <a href='/signup'>Sign Up</a></p>";
                resultLiteral.Text = st;
                Session.Remove("login_required");
            }
        }

        private void HandleLogin()
        {
            string email = (Request.Form["username"] ?? string.Empty).Trim();
            string password = Request.Form["password"] ?? string.Empty;
            int retryAfterSeconds;
            if (!AuthRateLimiter.IsAllowed(Request, "user-login", email, out retryAfterSeconds))
            {
                Response.StatusCode = 429;
                Response.AddHeader("Retry-After", retryAfterSeconds.ToString());
                SetLoginError("Too many login attempts. Try again later.");
                return;
            }

            if (email.Length > 300 || !UtilFunctionsClass.ValidateEmail(email) ||
                string.IsNullOrEmpty(password) || password.Length > 128)
            {
                AuthRateLimiter.RecordFailure(Request, "user-login", email);
                SetLoginError("Username or password is incorrect.");
                return;
            }

            string username = null;
            string storedPassword = null;
            bool resetRequired = false;
            int sessionVersion = 0;
            const string query = @"
SELECT TOP (2) [username], [password], [password_reset_required], [session_version]
FROM dbo.[user]
WHERE [email] = @Email;";

            using (SqlConnection connection = Helper.ConnectToDb())
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar, 300).Value = email;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        username = reader.GetString(0);
                        storedPassword = reader.GetString(1);
                        resetRequired = reader.GetBoolean(2);
                        sessionVersion = reader.GetInt32(3);
                    }
                    if (reader.Read())
                    {
                        username = null;
                    }
                }
            }

            PasswordVerificationResult verification = PasswordSecurity.Verify(password, storedPassword);
            if (username == null || verification == PasswordVerificationResult.Failed)
            {
                AuthRateLimiter.RecordFailure(Request, "user-login", email);
                SetLoginError("Username or password is incorrect.");
                return;
            }

            AuthRateLimiter.Reset(Request, "user-login", email);
            if (verification == PasswordVerificationResult.LegacyPlaintextSuccess)
            {
                string migratedHash = PasswordSecurity.HashPassword(password);
                using (SqlConnection connection = Helper.ConnectToDb())
                using (SqlCommand command = new SqlCommand(@"
UPDATE dbo.[user]
SET [password] = @PasswordHash,
    [password_reset_required] = 1,
    [session_version] = [session_version] + 1
WHERE [username] = @Username AND [password] = @LegacyPassword;", connection))
                {
                    command.Parameters.Add("@PasswordHash", System.Data.SqlDbType.NVarChar, 300).Value = migratedHash;
                    command.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar, 300).Value = username;
                    command.Parameters.Add("@LegacyPassword", System.Data.SqlDbType.NVarChar, 300).Value = password;
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                resetRequired = true;
            }
            else if (verification == PasswordVerificationResult.SuccessRehashNeeded)
            {
                string refreshedHash = PasswordSecurity.HashPassword(password);
                using (SqlConnection connection = Helper.ConnectToDb())
                using (SqlCommand command = new SqlCommand(
                    "UPDATE dbo.[user] SET [password] = @PasswordHash WHERE [username] = @Username;",
                    connection))
                {
                    command.Parameters.Add("@PasswordHash", System.Data.SqlDbType.NVarChar, 300).Value = refreshedHash;
                    command.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar, 300).Value = username;
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

            if (resetRequired)
            {
                Session["pending_password_reset_username"] = username;
                Session["pending_password_reset_email"] = email;
                Session["pending_password_reset_expires"] = DateTime.UtcNow.AddMinutes(10);
                PasswordResetRequired = true;
                LoggedInUsername = username;
                SetStatus("Choose a new password to finish signing in.");
                return;
            }

            UtilFunctionsClass.BeginAuthenticatedSession(Session, username, email, sessionVersion);
            Session.Remove("login_required");
            LoginSucceeded = true;
            LoggedInUsername = username;
            SetStatus("Login successful.");
        }

        private void SetLoginError(string message)
        {
            SetStatus(message, true);
        }

        private void SetStatus(string message, bool isError = false)
        {
            string color = isError ? "red" : "green";
            st = "<table dir='ltr' border='1'><tr><th style='color:" + color + "'>" +
                System.Web.HttpUtility.HtmlEncode(message) + "</th></tr></table>";
            resultLiteral.Text = st;
        }

        private string SafeReturnPage(string page)
        {
            switch ((page ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "home":
                case "create":
                case "my":
                case "account":
                    return page.Trim().ToLowerInvariant();
                default:
                    return "my";
            }
        }
    }
}
