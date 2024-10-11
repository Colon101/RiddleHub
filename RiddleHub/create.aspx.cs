using System;
using System.Data.SqlClient;
using System.Diagnostics;
using UtilFunctions;

namespace RiddleHub
{
    public partial class Create : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            bool loggedIn = UtilFunctionsClass.IsLoggedIn(Session);
            if (!loggedIn)
            {
                Session["permission"] = true;
                Response.Redirect("/login");
            }

            if (Request.Form["submit"] == null)
            {
                return;
            }
            else
            {
                if (!UtilFunctionsClass.IsLoggedIn(Session))
                {
                    Session["permission"] = true;
                    Response.Redirect("/login");
                }
                if (!string.IsNullOrEmpty(Request.Form["hint"]) || (Request.Form["hint"].Length != 0))
                {
                    string query = "INSERT INTO dbo.[riddle] (riddle_text, riddle_hint, answer, username) VALUES (@Text, @Hint, @Answer, @Username);";

                    using (SqlConnection conn = Helper.ConnectToDb("db.mdf"))
                    {
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.Add("@Text", System.Data.SqlDbType.NVarChar).Value = Request.Form["riddle"];
                        cmd.Parameters.Add("@Hint", System.Data.SqlDbType.NVarChar).Value = Request.Form["hint"];
                        cmd.Parameters.Add("@Answer", System.Data.SqlDbType.NVarChar).Value = Request.Form["answer"];
                        cmd.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar).Value = Session["username"];

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    string query = "INSERT INTO dbo.[riddle] (riddle_text, answer, username) VALUES (@Text, @Answer, @Username);";
                    using (SqlConnection conn = Helper.ConnectToDb("db.mdf"))
                    {
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.Add("@Text", System.Data.SqlDbType.NVarChar).Value = Request.Form["riddle"];
                        cmd.Parameters.Add("@Answer", System.Data.SqlDbType.NVarChar).Value = Request.Form["answer"];
                        cmd.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar).Value = Session["username"];

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}