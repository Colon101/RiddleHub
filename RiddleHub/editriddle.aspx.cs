using System;
using System.Data.SqlClient;
using UtilFunctions;

namespace RiddleHub
{
    public partial class editriddle : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.RequestType == "POST")
            {
                HandlePost();
                return;
            }

            if (string.IsNullOrEmpty(Request.Params["id"]))
            {
                Response.StatusCode = 400;
                Response.Redirect("/my");
                return;
            }
            string id = Request.Params["id"];
            riddleInfo.Text += id;
        }

        private void HandlePost()
        {
            if (!UtilFunctionsClass.IsLoggedIn(Session))
            {
                Response.StatusCode = 401;
                Response.Write("not logged in");
                Response.End();
                return;
            }

            if ((Request.Form["action"] ?? "").Trim() != "delete")
            {
                Response.StatusCode = 400;
                Response.Write("invalid action");
                Response.End();
                return;
            }

            if (!int.TryParse(Request.Form["id"], out int riddleId))
            {
                Response.StatusCode = 400;
                Response.Write("invalid id");
                Response.End();
                return;
            }

            string query = "DELETE FROM dbo.[riddle] WHERE riddle_id = @RiddleId AND username = @Username";
            using (SqlConnection conn = Helper.ConnectToDb("db.mdf"))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.Add("@RiddleId", System.Data.SqlDbType.Int).Value = riddleId;
                cmd.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar).Value = Session["username"];
                conn.Open();
                int rows = cmd.ExecuteNonQuery();
                if (rows == 0)
                {
                    Response.StatusCode = 404;
                    Response.Write("riddle not found");
                }
                else
                {
                    Response.StatusCode = 200;
                    Response.Write("deleted");
                }
            }
            Response.End();
        }
    }
}
