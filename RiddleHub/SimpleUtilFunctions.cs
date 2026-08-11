using System;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.SessionState;

namespace UtilFunctions
{
    public class UtilFunctionsClass
    {
        public static bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            const string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email.Trim(), emailPattern);
        }

        public static string ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return "Password is Empty";
            if (password.Length < 8) return "Password is too short (8 characters or more)";
            if (password.Length > 128) return "Password is too long (128 characters or fewer)";
            if (!password.Any(char.IsDigit)) return "Password requires at least one digit";
            if (!password.Any(char.IsUpper)) return "Password requires at least one uppercase character";
            return "Valid";
        }

        public static bool ValidUserName(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            const string userNamePattern = "^[A-Za-z0-9_]+$";
            return username.Length <= 300 && Regex.IsMatch(username.Trim(), userNamePattern);
        }

        public static bool IsLoggedIn(HttpSessionState session)
        {
            string email = Convert.ToString(session["email"]);
            string username = Convert.ToString(session["username"]);
            int sessionVersion;
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(username) ||
                !int.TryParse(Convert.ToString(session["session_version"]), out sessionVersion))
            {
                return false;
            }

            const string query = @"
SELECT COUNT(*)
FROM dbo.[user]
WHERE [username] = @Username
  AND [email] = @Email
  AND [session_version] = @SessionVersion
  AND [password_reset_required] = 0;";

            try
            {
                using (SqlConnection connection = Helper.ConnectToDb())
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar, 300).Value = username;
                    command.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar, 300).Value = email;
                    command.Parameters.Add("@SessionVersion", System.Data.SqlDbType.Int).Value = sessionVersion;
                    connection.Open();
                    return (int)command.ExecuteScalar() == 1;
                }
            }
            catch (SqlException)
            {
                return false;
            }
        }

        public static void BeginAuthenticatedSession(
            HttpSessionState session,
            string username,
            string email,
            int sessionVersion)
        {
            session.Remove("pending_password_reset_username");
            session.Remove("pending_password_reset_email");
            session.Remove("pending_password_reset_expires");
            session["username"] = username;
            session["email"] = email;
            session["session_version"] = sessionVersion;
        }

        public static void LogOut(HttpSessionState session)
        {
            session.Clear();
            session.Abandon();
        }
    }
}
