using System;
using System.Data.SqlClient;
using UtilFunctions;

namespace RiddleHub
{
    public partial class signup : System.Web.UI.Page
    {
        public string st;
        protected void Page_Load(object sender, EventArgs e)
        {
            bool loggedin = UtilFunctionsClass.IsLoggedIn(Session);
            if (loggedin)
            {
                Response.Redirect("/my.aspx");
            }
            if (Request.Form["submit"] == null)
            {
                return;
            }
            string email = Request.Form["email"];
            string password = Request.Form["password"];
            string username = Request.Form["username"];
            if (UtilFunctionsClass.ValidUserName(username))
            {
                st += "<table dir ='ltr' border ='1'>";
                st += "<tr><th style='color:red'> Error </th></tr>";
                st += $"<tr><td>Error:</td><td>Email or password not found</td></tr>";
                st += "</table>";
            }
            else if (UtilFunctionsClass.ValidatePassword(password) != "Valid")
            {
                st += "<table dir ='ltr' border ='1'>";
                st += "<tr><th style='color:red'> Error </th></tr>";
                st += $"<tr><td>Error:</td><td>{UtilFunctionsClass.ValidatePassword(password)}</td></tr>";
                st += "</table>";
            }
            else if (!UtilFunctionsClass.ValidateEmail(email))
            {
                st += "<table dir ='ltr' border ='1'>";
                st += "<tr><th style='color:red'> Error </th></tr>";
                st += $"<tr><td>Error:</td><td>Invalid Email</td></tr>";
                st += "</table>";
            }
            else
            {

                string query2 = "SELECT COUNT(*) FROM dbo.[user] WHERE email = @Email OR username = @Username;";
                bool isAccountUsed = false;

                using (SqlConnection conn = Helper.ConnectToDb("db.mdf"))
                {
                    SqlCommand cmd = new SqlCommand(query2, conn);
                    cmd.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar).Value = email;
                    cmd.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar).Value = username;

                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();
                    isAccountUsed = count > 0;
                }
                if (isAccountUsed)
                {
                    st += "<table dir ='ltr' border ='1'>";
                    st += "<tr><th style='color: red;'>Error</th></tr>";
                    st += $"<tr><td>Error</td><td>Username or Email is already in use</td></tr>";
                    st += "</table>";
                }
                else
                {
                    string query = "INSERT INTO dbo.[user] (username, password, email) VALUES (@Username, @Password, @Email);";
                    using (SqlConnection conn = Helper.ConnectToDb("db.mdf"))
                    {
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar).Value = username;
                        cmd.Parameters.Add("@Password", System.Data.SqlDbType.NVarChar).Value = password;
                        cmd.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar).Value = email;
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    st += "<table dir ='ltr' border ='1'>";
                    st += "<tr><th>Success</th></tr>";
                    st += "</table>";
                    Session["username"] = username;
                    Session["email"] = email;
                    Session["password"] = password;

                }

                resultLiteral.Text = st;
            }
        }
    }
}