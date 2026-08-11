using System;
using System.Data.SqlClient;
using RiddleHub.Security;
using UtilFunctions;

namespace RiddleHub
{
    public partial class signup : System.Web.UI.Page
    {
        public string st = string.Empty;
        public string ReturnPage = "my";
        public string CsrfTokenEncoded = string.Empty;
        public bool SignupSucceeded;
        public string SignedUpUsername = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            ReturnPage = SafeReturnPage(Request.Params["return"]);
            CsrfTokenEncoded = CsrfProtection.GetEncodedToken(Session);
            if (UtilFunctionsClass.IsLoggedIn(Session))
            {
                Response.Redirect("/my.aspx");
                return;
            }
            if (Request.Form["submit"] == null)
            {
                return;
            }
            if (CsrfProtection.RejectInvalidPost(Request, Response, Session))
            {
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            string email = (Request.Form["email"] ?? string.Empty).Trim();
            string password = Request.Form["password"] ?? string.Empty;
            string username = (Request.Form["username"] ?? string.Empty).Trim();
            int retryAfterSeconds;
            if (!AuthRateLimiter.IsAllowed(Request, "signup", email, out retryAfterSeconds))
            {
                Response.StatusCode = 429;
                Response.AddHeader("Retry-After", retryAfterSeconds.ToString());
                SetStatus("Too many signup attempts. Try again later.", true);
                return;
            }

            string passwordValidation = UtilFunctionsClass.ValidatePassword(password);
            if (!UtilFunctionsClass.ValidUserName(username))
            {
                AuthRateLimiter.RecordFailure(Request, "signup", email);
                SetStatus("Invalid username. Use letters, numbers, and underscores only.", true);
                return;
            }
            if (passwordValidation != "Valid")
            {
                AuthRateLimiter.RecordFailure(Request, "signup", email);
                SetStatus(passwordValidation, true);
                return;
            }
            if (email.Length > 300 || !UtilFunctionsClass.ValidateEmail(email))
            {
                AuthRateLimiter.RecordFailure(Request, "signup", email);
                SetStatus("Invalid email.", true);
                return;
            }

            const string duplicateQuery =
                "SELECT COUNT(*) FROM dbo.[user] WHERE [email] = @Email OR [username] = @Username;";
            using (SqlConnection connection = Helper.ConnectToDb())
            using (SqlCommand command = new SqlCommand(duplicateQuery, connection))
            {
                command.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar, 300).Value = email;
                command.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar, 300).Value = username;
                connection.Open();
                if ((int)command.ExecuteScalar() > 0)
                {
                    AuthRateLimiter.RecordFailure(Request, "signup", email);
                    Response.StatusCode = 409;
                    SetStatus("Username or email is already in use.", true);
                    return;
                }
            }

            const string insertQuery = @"
INSERT INTO dbo.[user]
    ([username], [password], [email], [password_reset_required], [session_version])
VALUES
    (@Username, @PasswordHash, @Email, 0, 1);";
            try
            {
                using (SqlConnection connection = Helper.ConnectToDb())
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar, 300).Value = username;
                    command.Parameters.Add("@PasswordHash", System.Data.SqlDbType.NVarChar, 300).Value =
                        PasswordSecurity.HashPassword(password);
                    command.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar, 300).Value = email;
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException exception)
            {
                if (exception.Number == 2601 || exception.Number == 2627)
                {
                    AuthRateLimiter.RecordFailure(Request, "signup", email);
                    Response.StatusCode = 409;
                    SetStatus("Username or email is already in use.", true);
                    return;
                }
                throw;
            }

            AuthRateLimiter.Reset(Request, "signup", email);
            UtilFunctionsClass.BeginAuthenticatedSession(Session, username, email, 1);
            SignupSucceeded = true;
            SignedUpUsername = username;
            SetStatus("Signup successful.");
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
