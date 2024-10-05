using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.SessionState;

namespace UtilFunctions
{
    class UtilFunctionsClass
    {
        public const string DBConnString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Kfir\\Documents\\RiddleHub\\RiddleHub\\RiddleHub\\App_Data\\Database.mdf;Integrated Security=True";
        public static bool ValidateEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }

            // Validate using regex for simplicity and thoroughness
            var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailRegex);
        }
        public static string ValidatePassword(string password)
        {

            if (string.IsNullOrEmpty(password)) return "Password is Empty";
            if (password.Length <= 8) return "Password is too short (8 characters or more)";
            if (!password.Any(char.IsDigit)) return "Password requires at least one digit";
            if (!password.Any(char.IsUpper)) return "Password requires at least one uppercase character";
            return "Valid";
        }
        public static bool ValidUserName(string username)
        {
            string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890_";
            foreach (char c in username) if (validChars.Contains(c)) return false;
            return true;
        }
        public static bool IsLoggedIn(HttpSessionState session)
        {
            if (session["email"] == null) return false;
            else if (session["password"] == null) return false;
            else if (session["username"] == null) return false;
            return true;
        }
        public static void LogOut(HttpSessionState session)
        {
            session["email"] = null;
            session["password"] = null;
            session["username"] = null;
        }
        public static string GetUsernameFromEmail(string email)
        {
            string query = @" SELECT username FROM dbo.[user] WHERE email = @Email";
            string username = null;
            using (SqlConnection conn = Helper.ConnectToDb("Database.mdf"))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar).Value = email;
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        username = reader.GetString(0);
                    }
                }
            }
            if (username != null)
            {

                return username;
            }
            return null;
        }
        public static void BECAREFULL_____THISCLEANSTHEDB_CleanDb()
        {

        }
    }
}