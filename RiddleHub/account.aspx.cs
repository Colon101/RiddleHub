using System;
using System.Data.SqlClient;
using System.Web;
using RiddleHub.Security;
using UtilFunctions;

namespace RiddleHub
{
    public partial class account : System.Web.UI.Page
    {
        public string CurrentUsernameEncoded = string.Empty;
        public string StatusMessageEncoded = string.Empty;
        public string CsrfTokenEncoded = string.Empty;
        public bool IsErrorStatus;
        public bool AccountDeleted;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!UtilFunctionsClass.IsLoggedIn(Session))
            {
                Session["login_required"] = true;
                Response.Redirect("/login.aspx?return=account&required=1");
                return;
            }

            CsrfTokenEncoded = CsrfProtection.GetEncodedToken(Session);
            string currentUsername = Convert.ToString(Session["username"]) ?? string.Empty;
            if (Request.Form["action"] != null)
            {
                if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) ||
                    CsrfProtection.RejectInvalidPost(Request, Response, Session))
                {
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }
                HandleAction(ref currentUsername);
            }

            username.Text = HttpUtility.HtmlEncode(currentUsername);
            CurrentUsernameEncoded = HttpUtility.HtmlEncode(currentUsername);
        }

        private void HandleAction(ref string currentUsername)
        {
            switch ((Request.Form["action"] ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "change_username":
                    ChangeUsername(ref currentUsername);
                    break;
                case "change_password":
                    ChangePassword(currentUsername);
                    break;
                case "delete_account":
                    DeleteAccount(currentUsername);
                    break;
                default:
                    SetStatus("Unknown account action.", true);
                    break;
            }
        }

        private void ChangeUsername(ref string currentUsername)
        {
            string newUsername = (Request.Form["new_username"] ?? string.Empty).Trim();
            if (!UtilFunctionsClass.ValidUserName(newUsername))
            {
                SetStatus("Invalid username. Use letters, numbers, and underscores only.", true);
                return;
            }
            if (string.Equals(newUsername, currentUsername, StringComparison.Ordinal))
            {
                SetStatus("New username is the same as your current username.");
                return;
            }

            string currentPasswordHash = null;
            string currentEmail = null;
            int newSessionVersion = 0;
            using (SqlConnection connection = Helper.ConnectToDb())
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    using (SqlCommand existsCommand = new SqlCommand(
                        "SELECT COUNT(*) FROM dbo.[user] WHERE [username] = @Username;",
                        connection,
                        transaction))
                    {
                        existsCommand.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar, 300).Value = newUsername;
                        if ((int)existsCommand.ExecuteScalar() > 0)
                        {
                            transaction.Rollback();
                            SetStatus("Username is already in use.", true);
                            return;
                        }
                    }

                    int currentSessionVersion = 0;
                    using (SqlCommand userCommand = new SqlCommand(@"
SELECT [password], [email], [session_version]
FROM dbo.[user]
WHERE [username] = @Username AND [password_reset_required] = 0;", connection, transaction))
                    {
                        userCommand.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar, 300).Value = currentUsername;
                        using (SqlDataReader reader = userCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                currentPasswordHash = reader.GetString(0);
                                currentEmail = reader.GetString(1);
                                currentSessionVersion = reader.GetInt32(2);
                            }
                        }
                    }
                    if (currentPasswordHash == null || currentEmail == null)
                    {
                        transaction.Rollback();
                        SetStatus("Current account was not found.", true);
                        return;
                    }

                    newSessionVersion = checked(currentSessionVersion + 1);
                    using (SqlCommand insertCommand = new SqlCommand(@"
INSERT INTO dbo.[user]
    ([username], [password], [email], [password_reset_required], [session_version])
VALUES
    (@NewUsername, @PasswordHash, @Email, 0, @SessionVersion);", connection, transaction))
                    {
                        insertCommand.Parameters.Add("@NewUsername", System.Data.SqlDbType.NVarChar, 300).Value = newUsername;
                        insertCommand.Parameters.Add("@PasswordHash", System.Data.SqlDbType.NVarChar, 300).Value = currentPasswordHash;
                        insertCommand.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar, 300).Value = currentEmail;
                        insertCommand.Parameters.Add("@SessionVersion", System.Data.SqlDbType.Int).Value = newSessionVersion;
                        insertCommand.ExecuteNonQuery();
                    }
                    using (SqlCommand updateRiddlesCommand = new SqlCommand(
                        "UPDATE dbo.[riddle] SET [username] = @NewUsername WHERE [username] = @CurrentUsername;",
                        connection,
                        transaction))
                    {
                        updateRiddlesCommand.Parameters.Add("@NewUsername", System.Data.SqlDbType.NVarChar, 300).Value = newUsername;
                        updateRiddlesCommand.Parameters.Add("@CurrentUsername", System.Data.SqlDbType.NVarChar, 300).Value = currentUsername;
                        updateRiddlesCommand.ExecuteNonQuery();
                    }
                    using (SqlCommand deleteCommand = new SqlCommand(
                        "DELETE FROM dbo.[user] WHERE [username] = @CurrentUsername;",
                        connection,
                        transaction))
                    {
                        deleteCommand.Parameters.Add("@CurrentUsername", System.Data.SqlDbType.NVarChar, 300).Value = currentUsername;
                        deleteCommand.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            UtilFunctionsClass.BeginAuthenticatedSession(Session, newUsername, currentEmail, newSessionVersion);
            currentUsername = newUsername;
            SetStatus("Username updated.");
        }

        private void ChangePassword(string currentUsername)
        {
            string currentPassword = Request.Form["current_password"] ?? string.Empty;
            string newPassword = Request.Form["new_password"] ?? string.Empty;
            string validation = UtilFunctionsClass.ValidatePassword(newPassword);
            if (currentPassword.Length > 128)
            {
                SetStatus("Current password is incorrect.", true);
                return;
            }
            if (validation != "Valid")
            {
                SetStatus(validation, true);
                return;
            }

            string storedHash = LoadPasswordHash(currentUsername);
            if (PasswordSecurity.Verify(currentPassword, storedHash) == PasswordVerificationResult.Failed)
            {
                SetStatus("Current password is incorrect.", true);
                return;
            }
            if (PasswordSecurity.Verify(newPassword, storedHash) != PasswordVerificationResult.Failed)
            {
                SetStatus("Choose a password different from your current password.", true);
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
WHERE [username] = @Username;", connection))
            {
                command.Parameters.Add("@PasswordHash", System.Data.SqlDbType.NVarChar, 300).Value =
                    PasswordSecurity.HashPassword(newPassword);
                command.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar, 300).Value = currentUsername;
                connection.Open();
                sessionVersion = Convert.ToInt32(command.ExecuteScalar());
            }
            UtilFunctionsClass.BeginAuthenticatedSession(
                Session,
                currentUsername,
                Convert.ToString(Session["email"]),
                sessionVersion);
            SetStatus("Password updated. Other sessions were signed out.");
        }

        private void DeleteAccount(string currentUsername)
        {
            string confirmation = Request.Form["confirm_password"] ?? string.Empty;
            if (confirmation.Length > 128)
            {
                SetStatus("Current password is incorrect.", true);
                return;
            }
            string storedHash = LoadPasswordHash(currentUsername);
            if (PasswordSecurity.Verify(confirmation, storedHash) == PasswordVerificationResult.Failed)
            {
                SetStatus("Current password is incorrect.", true);
                return;
            }

            using (SqlConnection connection = Helper.ConnectToDb())
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    using (SqlCommand deleteRiddlesCommand = new SqlCommand(
                        "DELETE FROM dbo.[riddle] WHERE [username] = @Username;",
                        connection,
                        transaction))
                    {
                        deleteRiddlesCommand.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar, 300).Value = currentUsername;
                        deleteRiddlesCommand.ExecuteNonQuery();
                    }
                    using (SqlCommand deleteUserCommand = new SqlCommand(
                        "DELETE FROM dbo.[user] WHERE [username] = @Username;",
                        connection,
                        transaction))
                    {
                        deleteUserCommand.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar, 300).Value = currentUsername;
                        deleteUserCommand.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            UtilFunctionsClass.LogOut(Session);
            AccountDeleted = true;
            SetStatus("Account deleted.");
        }

        private static string LoadPasswordHash(string username)
        {
            using (SqlConnection connection = Helper.ConnectToDb())
            using (SqlCommand command = new SqlCommand(
                "SELECT [password] FROM dbo.[user] WHERE [username] = @Username AND [password_reset_required] = 0;",
                connection))
            {
                command.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar, 300).Value = username;
                connection.Open();
                return Convert.ToString(command.ExecuteScalar());
            }
        }

        private void SetStatus(string message, bool isError = false)
        {
            IsErrorStatus = isError;
            StatusMessageEncoded = HttpUtility.HtmlEncode(message);
        }
    }
}
