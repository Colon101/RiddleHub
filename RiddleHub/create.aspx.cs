using System;
using System.Data.SqlClient;
using RiddleHub.Security;
using UtilFunctions;

namespace RiddleHub
{
    public partial class Create : System.Web.UI.Page
    {
        public string CsrfTokenEncoded = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!UtilFunctionsClass.IsLoggedIn(Session))
            {
                Session["login_required"] = true;
                Response.Redirect("/login.aspx?return=create&required=1");
                return;
            }

            CsrfTokenEncoded = CsrfProtection.GetEncodedToken(Session);
            if (Request.Form["submit"] == null)
            {
                return;
            }
            if (CsrfProtection.RejectInvalidPost(Request, Response, Session))
            {
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            string text = (Request.Form["riddle"] ?? string.Empty).Trim();
            string hint = (Request.Form["hint"] ?? string.Empty).Trim();
            string answer = (Request.Form["answer"] ?? string.Empty).Trim();
            if (text.Length == 0 || text.Length > 2000 ||
                hint.Length > 100 ||
                answer.Length == 0 || answer.Length > 100)
            {
                Response.StatusCode = 400;
                Response.TrySkipIisCustomErrors = true;
                Response.Write("Invalid riddle fields.");
                return;
            }

            const string query = @"
INSERT INTO dbo.[riddle] (riddle_text, riddle_hint, answer, username)
VALUES (@Text, @Hint, @Answer, @Username);";
            using (SqlConnection connection = Helper.ConnectToDb())
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@Text", System.Data.SqlDbType.NVarChar, 2000).Value = text;
                command.Parameters.Add("@Hint", System.Data.SqlDbType.NVarChar, 100).Value =
                    hint.Length == 0 ? (object)DBNull.Value : hint;
                command.Parameters.Add("@Answer", System.Data.SqlDbType.NVarChar, 100).Value = answer;
                command.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar, 300).Value =
                    Convert.ToString(Session["username"]);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
