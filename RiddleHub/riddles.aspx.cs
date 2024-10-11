using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace RiddleHub
{
    public partial class riddles : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Clear();
            Response.ContentType = "application/json; charset=utf-8";
            Response.Write(GenerateJson());
            Response.End();
        }

        public static string GenerateJson()
        {
            List<Riddle> riddleList = new List<Riddle>();
            string query = @"
                SELECT riddle_id, riddle_text, riddle_hint, answer, username
                FROM dbo.[riddle]";

            using (SqlConnection conn = Helper.ConnectToDb("db.mdf"))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Riddle riddle = new Riddle
                        {
                            riddle_id = reader.GetInt32(0),
                            riddle_text = reader.GetString(1),
                            riddle_hint = reader.IsDBNull(2) ? null : reader.GetString(2),
                            answer = reader.GetString(3),
                            username = reader.GetString(4)
                        };
                        riddleList.Add(riddle);
                    }
                }
            }

            RiddleObject riddleObject = new RiddleObject
            {
                riddleList = riddleList
            };

            string json = JsonConvert.SerializeObject(riddleObject);
            return json;
        }
    }

    public class Riddle
    {
        public int riddle_id { get; set; }
        public string riddle_text { get; set; }
        public string riddle_hint { get; set; }
        public string answer { get; set; }
        public string username { get; set; }
    }

    public class User
    {
        public string username { get; set; }
        public string password { get; set; }
        public string email { get; set; }
    }

    public class RiddleObject
    {
        public List<Riddle> riddleList { get; set; }
    }
}
