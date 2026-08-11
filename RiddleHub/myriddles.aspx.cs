using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Newtonsoft.Json;
using UtilFunctions;

namespace RiddleHub
{
    public partial class myriddles : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Clear();
            Response.ContentType = "application/json; charset=utf-8";

            if (!UtilFunctionsClass.IsLoggedIn(Session))
            {
                Response.StatusCode = 401;
                Response.Write("{\"error\":\"not logged in\"}");
                Response.End();
                return;
            }

            string username = Convert.ToString(Session["username"]);
            if (string.IsNullOrWhiteSpace(username))
            {
                Response.StatusCode = 401;
                Response.Write("{\"error\":\"not logged in\"}");
                Response.End();
                return;
            }

            string json = GenerateJson(username);
            Response.Write(json);
            Response.End();
        }

        public static string GenerateJson(string username)
        {
            List<UserRiddle> userRiddles = new List<UserRiddle>();
            string query = @"SELECT riddle_id, riddle_text, riddle_hint, answer FROM dbo.[riddle] WHERE username = @Username";

            using (SqlConnection conn = Helper.ConnectToDb())
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar, 300).Value = username;

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        UserRiddle riddle = new UserRiddle
                        {
                            riddle_id = reader.GetInt32(0),
                            riddle_text = reader.GetString(1),
                            riddle_hint = reader.IsDBNull(2) ? null : reader.GetString(2),
                            answer = reader.GetString(3)
                        };
                        userRiddles.Add(riddle);
                    }
                }
            }

            UserRiddleObject userRiddleObject = new UserRiddleObject { userRiddles = userRiddles };

            return JsonConvert.SerializeObject(userRiddleObject);
        }
    }
    class UserRiddle
    {
        public int riddle_id { get; set; }
        public string riddle_text { get; set; }
        public string riddle_hint { get; set; }
        public string answer { get; set; }
    }
    class UserRiddleObject
    {
        public List<UserRiddle> userRiddles { get; set; }
    }
}
