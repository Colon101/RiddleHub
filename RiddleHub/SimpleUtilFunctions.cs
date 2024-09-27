using System.Linq;
using System.Text.RegularExpressions;

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
    }
}