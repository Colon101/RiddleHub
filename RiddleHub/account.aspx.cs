using System;
using System.Data.SqlClient;
using System.Web;
using UtilFunctions;

namespace RiddleHub
{
    public partial class account : System.Web.UI.Page
    {
        public string CurrentUsernameEncoded = string.Empty;
        public string StatusMessageEncoded = string.Empty;
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

            string currentUsername = Convert.ToString(Session["username"]) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(currentUsername))
            {
                UtilFunctionsClass.LogOut(Session);
                Session["login_required"] = true;
                Response.Redirect("/login.aspx?return=account&required=1");
                return;
            }

            if (Request.Form["action"] != null)
            {
                HandleAction(ref currentUsername);
            }

            username.Text = currentUsername;
            CurrentUsernameEncoded = HttpUtility.HtmlEncode(currentUsername);
        }

        private void HandleAction(ref string currentUsername)
        {
            string action = (Request.Form["action"] ?? string.Empty).Trim().ToLowerInvariant();
            switch (action)
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

            using (SqlConnection conn = Helper.ConnectToDb("db.mdf"))
            {
                conn.Open();
                SqlTransaction tx = conn.BeginTransaction();
                try
                {
                    using (SqlCommand existsCmd = new SqlCommand("SELECT COUNT(*) FROM dbo.[user] WHERE username = @Username;", conn, tx))
                    {
                        existsCmd.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar, 300).Value = newUsername;
                        if ((int)existsCmd.ExecuteScalar() > 0)
                        {
                            tx.Rollback();
                            SetStatus("Username is already in use.", true);
                            return;
                        }
                    }

                    string currentPassword = null;
                    string currentEmail = null;
                    using (SqlCommand userCmd = new SqlCommand("SELECT [password], [email] FROM dbo.[user] WHERE username = @Username;", conn, tx))
                    {
                        userCmd.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar, 300).Value = currentUsername;
                        using (SqlDataReader reader = userCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                currentPassword = reader.GetString(0);
                                currentEmail = reader.GetString(1);
                            }
                        }
                    }

                    if (currentPassword == null || currentEmail == null)
                    {
                        tx.Rollback();
                        SetStatus("Current account was not found.", true);
                        return;
                    }

                    using (SqlCommand insertCmd = new SqlCommand(
                        "INSERT INTO dbo.[user] ([username], [password], [email]) VALUES (@NewUsername, @Password, @Email);",
                        conn,
                        tx))
                    {
                        insertCmd.Parameters.Add("@NewUsername", System.Data.SqlDbType.NVarChar, 300).Value = newUsername;
                        insertCmd.Parameters.Add("@Password", System.Data.SqlDbType.NVarChar, 300).Value = currentPassword;
                        insertCmd.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar, 300).Value = currentEmail;
                        insertCmd.ExecuteNonQuery();
                    }

                    using (SqlCommand updateRiddlesCmd = new SqlCommand(
                        "UPDATE dbo.[riddle] SET username = @NewUsername WHERE username = @CurrentUsername;",
                        conn,
                        tx))
                    {
                        updateRiddlesCmd.Parameters.Add("@NewUsername", System.Data.SqlDbType.NVarChar, 300).Value = newUsername;
                        updateRiddlesCmd.Parameters.Add("@CurrentUsername", System.Data.SqlDbType.NVarChar, 300).Value = currentUsername;
                        updateRiddlesCmd.ExecuteNonQuery();
                    }

                    using (SqlCommand deleteOldUserCmd = new SqlCommand(
                        "DELETE FROM dbo.[user] WHERE username = @CurrentUsername;",
                        conn,
                        tx))
                    {
                        deleteOldUserCmd.Parameters.Add("@CurrentUsername", System.Data.SqlDbType.NVarChar, 300).Value = currentUsername;
                        deleteOldUserCmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }

            Session["username"] = newUsername;
            currentUsername = newUsername;
            SetStatus("Username updated.");
        }

        private void ChangePassword(string currentUsername)
        {
            string currentPassword = Request.Form["current_password"] ?? string.Empty;
            string newPassword = Request.Form["new_password"] ?? string.Empty;

            string passwordValidation = UtilFunctionsClass.ValidatePassword(newPassword);
            if (passwordValidation != "Valid")
            {
                SetStatus(passwordValidation, true);
                return;
            }

            using (SqlConnection conn = Helper.ConnectToDb("db.mdf"))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE dbo.[user] SET [password] = @NewPassword WHERE [username] = @Username AND [password] = @CurrentPassword;",
                    conn))
                {
                    cmd.Parameters.Add("@NewPassword", System.Data.SqlDbType.NVarChar, 300).Value = newPassword;
                    cmd.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar, 300).Value = currentUsername;
                    cmd.Parameters.Add("@CurrentPassword", System.Data.SqlDbType.NVarChar, 300).Value = currentPassword;
                    int rows = cmd.ExecuteNonQuery();
                    if (rows == 0)
                    {
                        SetStatus("Current password is incorrect.", true);
                        return;
                    }
                }
            }

            Session["password"] = newPassword;
            SetStatus("Password updated.");
        }

        private void DeleteAccount(string currentUsername)
        {
            string confirmPassword = Request.Form["confirm_password"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(confirmPassword))
            {
                SetStatus("Password confirmation is required to delete the account.", true);
                return;
            }

            using (SqlConnection conn = Helper.ConnectToDb("db.mdf"))
            {
                conn.Open();
                SqlTransaction tx = conn.BeginTransaction();
                try
                {
                    using (SqlCommand deleteRiddlesCmd = new SqlCommand(
                        "DELETE FROM dbo.[riddle] WHERE username = @Username;",
                        conn,
                        tx))
                    {
                        deleteRiddlesCmd.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar, 300).Value = currentUsername;
                        deleteRiddlesCmd.ExecuteNonQuery();
                    }

                    int deletedUsers;
                    using (SqlCommand deleteUserCmd = new SqlCommand(
                        "DELETE FROM dbo.[user] WHERE username = @Username AND [password] = @Password;",
                        conn,
                        tx))
                    {
                        deleteUserCmd.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar, 300).Value = currentUsername;
                        deleteUserCmd.Parameters.Add("@Password", System.Data.SqlDbType.NVarChar, 300).Value = confirmPassword;
                        deletedUsers = deleteUserCmd.ExecuteNonQuery();
                    }

                    if (deletedUsers == 0)
                    {
                        tx.Rollback();
                        SetStatus("Current password is incorrect.", true);
                        return;
                    }

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }

            UtilFunctionsClass.LogOut(Session);
            AccountDeleted = true;
            SetStatus("Account deleted.");
        }

        private void SetStatus(string message, bool isError = false)
        {
            IsErrorStatus = isError;
            StatusMessageEncoded = HttpUtility.HtmlEncode(message);
        }
    }
}
