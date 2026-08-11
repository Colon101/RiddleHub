using System;
using System.Data.SqlClient;
using RiddleHub.Security;
using UtilFunctions;

namespace RiddleHub
{
    public partial class editriddle : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            {
                HandlePost();
                return;
            }

            int riddleId;
            if (!int.TryParse(Request.QueryString["id"], out riddleId) || riddleId <= 0)
            {
                Response.StatusCode = 400;
                riddleInfo.Text = "Invalid riddle id.";
                return;
            }
            riddleInfo.Text = riddleId.ToString();
        }

        private void HandlePost()
        {
            if (!UtilFunctionsClass.IsLoggedIn(Session))
            {
                WriteResult(401, "not logged in");
                return;
            }
            if (CsrfProtection.RejectInvalidPost(Request, Response, Session))
            {
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
            if (!string.Equals((Request.Form["action"] ?? string.Empty).Trim(), "delete", StringComparison.Ordinal))
            {
                WriteResult(400, "invalid action");
                return;
            }

            int riddleId;
            if (!int.TryParse(Request.Form["id"], out riddleId) || riddleId <= 0)
            {
                WriteResult(400, "invalid id");
                return;
            }

            const string query =
                "DELETE FROM dbo.[riddle] WHERE [riddle_id] = @RiddleId AND [username] = @Username;";
            using (SqlConnection connection = Helper.ConnectToDb())
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@RiddleId", System.Data.SqlDbType.Int).Value = riddleId;
                command.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar, 300).Value =
                    Convert.ToString(Session["username"]);
                connection.Open();
                int rows = command.ExecuteNonQuery();
                WriteResult(rows == 0 ? 404 : 200, rows == 0 ? "riddle not found" : "deleted");
            }
        }

        private void WriteResult(int statusCode, string message)
        {
            Response.StatusCode = statusCode;
            Response.TrySkipIisCustomErrors = true;
            Response.ContentType = "text/plain; charset=utf-8";
            Response.Write(message);
        }
    }
}
