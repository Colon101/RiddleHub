using System;
using System.Data.SqlClient;
using System.Web;
using RiddleHub.Security;
using UtilFunctions;

namespace RiddleHub
{
    public partial class resetpassword : System.Web.UI.Page
    {
        public string ReturnPage = "my";
        public string CsrfTokenEncoded = string.Empty;
        public string StatusMessageEncoded = string.Empty;
        public bool IsErrorStatus;
        public bool ResetSucceeded;
        public string LoggedInUsername = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            ReturnPage = SafeReturnPage(Request.Params["return"]);
            CsrfTokenEncoded = CsrfProtection.GetEncodedToken(Session);
            string username;
            string email;
            if (!TryGetPendingReset(out username, out email))
            {
                Response.StatusCode = 401;
                SetStatus("Your password-reset session expired. Sign in again.", true);
                return;
            }

            if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            if (CsrfProtection.RejectInvalidPost(Request, Response, Session))
            {
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            string newPassword = Request.Form["new_password"] ?? string.Empty;
            string confirmation = Request.Form["confirm_password"] ?? string.Empty;
            string validation = UtilFunctionsClass.ValidatePassword(newPassword);
            if (validation != "Valid")
            {
                SetStatus(validation, true);
                return;
            }
            if (confirmation.Length > 128 || !PasswordSecurity.FixedTimeEqualsUtf8(newPassword, confirmation))
            {
                SetStatus("Password confirmation does not match.", true);
                return;
            }

            string currentHash = null;
            using (SqlConnection connection = Helper.ConnectToDb())
            using (SqlCommand command = new SqlCommand(
                "SELECT [password] FROM dbo.[user] WHERE [username] = @Username AND [email] = @Email AND [password_reset_required] = 1;",
                connection))
            {
                command.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar, 300).Value = username;
                command.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar, 300).Value = email;
                connection.Open();
                currentHash = Convert.ToString(command.ExecuteScalar());
            }

            if (string.IsNullOrEmpty(currentHash))
            {
                SetStatus("The account is no longer eligible for this reset.", true);
                return;
            }
            if (PasswordSecurity.Verify(newPassword, currentHash) != PasswordVerificationResult.Failed)
            {
                SetStatus("Choose a password different from the exposed legacy password.", true);
                return;
            }

            int sessionVersion;
            using (SqlConnection connection = Helper.ConnectToDb())
            using (SqlCommand command = new SqlCommand(@"
UPDATE dbo.[user]
SET [password] = @PasswordHash,
    [password_reset_required] = 0,
    [session_version] = [session_version] + 1
OUTPUT INSERTED.[session_version]
WHERE [username] = @Username AND [email] = @Email AND [password_reset_required] = 1;", connection))
            {
                command.Parameters.Add("@PasswordHash", System.Data.SqlDbType.NVarChar, 300).Value =
                    PasswordSecurity.HashPassword(newPassword);
                command.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar, 300).Value = username;
                command.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar, 300).Value = email;
                connection.Open();
                object result = command.ExecuteScalar();
                if (result == null)
                {
                    SetStatus("The password reset could not be completed.", true);
                    return;
                }
                sessionVersion = Convert.ToInt32(result);
            }

            UtilFunctionsClass.BeginAuthenticatedSession(Session, username, email, sessionVersion);
            ResetSucceeded = true;
            LoggedInUsername = username;
            SetStatus("Password reset complete.");
        }

        private bool TryGetPendingReset(out string username, out string email)
        {
            username = Convert.ToString(Session["pending_password_reset_username"]);
            email = Convert.ToString(Session["pending_password_reset_email"]);
            DateTime expires;
            object expiresValue = Session["pending_password_reset_expires"];
            if (expiresValue is DateTime)
            {
                expires = (DateTime)expiresValue;
            }
            else if (!DateTime.TryParse(Convert.ToString(expiresValue), out expires))
            {
                return false;
            }
            return !string.IsNullOrWhiteSpace(username) &&
                !string.IsNullOrWhiteSpace(email) &&
                expires.ToUniversalTime() > DateTime.UtcNow;
        }

        private void SetStatus(string message, bool isError = false)
        {
            IsErrorStatus = isError;
            StatusMessageEncoded = HttpUtility.HtmlEncode(message);
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
