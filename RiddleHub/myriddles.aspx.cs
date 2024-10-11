using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using UtilFunctions;
namespace RiddleHub
{
    public partial class myriddles : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string username = Request.Params["username"];
            Response.Clear();
            Response.ContentType = "application/json; charset=utf-8";
            string json = GenerateJson(Session, Request.Params["username"]);
            if (json == "{\"error\":\"username not found\"}")
            {
                Response.StatusCode = 404;
            }
            else if (json == "{\"error\":\"username is empty\"}")
            {
                Response.StatusCode = 400;
            }
            Response.Write(json);
            Response.End();
        }
        public static string GenerateJson(HttpSessionState session, string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return "{\"error\":\"username is empty\"}";
            }

            List<UserRiddle> userRiddles = new List<UserRiddle>();
            string query = @"SELECT riddle_id, riddle_text, riddle_hint, answer FROM dbo.[riddle] WHERE username = @Username";

            using (SqlConnection conn = Helper.ConnectToDb("db.mdf"))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", username);

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
                if (userRiddles.Count  == 0)
                {
                    
                    return "{\"error\":\"username not found\"}";
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