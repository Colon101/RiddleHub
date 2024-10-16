﻿using System;
using System.Data.SqlClient;
using UtilFunctions;
namespace RiddleHub
{
    public partial class Login : System.Web.UI.Page
    {
        public string st;
        protected void Page_Load(object sender, EventArgs e)
        {




            if (Request.Form["submit"] != null)
            {


                string email = Request.Form["username"];
                string password = Request.Form["password"];
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    st += "<table dir ='ltr' border ='1'>";
                    st += "<tr><th style='color:red'> Error </th></tr>";
                    st += $"<tr><td>Error:</td><td>Email or password not found</td></tr>";
                    st += "</table>";
                }
                else if (!UtilFunctionsClass.ValidateEmail(email))
                {
                    st += "<table dir ='ltr' border ='1'>";
                    st += "<tr><th style='color:red'> Error </th></tr>";
                    st += $"<tr><td>Error:</td><td>invalid email</td></tr>";
                    st += "</table>";
                }
                else
                {
                    string query = "SELECT COUNT(*) FROM dbo.[user] WHERE email = @Email AND password = @Password;";
                    bool login = false;

                    using (SqlConnection conn = Helper.ConnectToDb("db.mdf"))
                    {
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar).Value = email;
                        cmd.Parameters.Add("@Password", System.Data.SqlDbType.NVarChar).Value = password;
                        conn.Open();
                        int count = (int)cmd.ExecuteScalar();
                        login = count > 0;
                    }

                    if (login)
                    {
                        st += "<table dir='ltr' border='1'>";
                        st += $"<tr><th style='color:green'>Success '{UtilFunctionsClass.GetUsernameFromEmail(email)}'</th></tr>";
                        st += "</table>";
                        Session["username"] = UtilFunctionsClass.GetUsernameFromEmail(email);
                        Session["email"] = email;
                        Session["password"] = password;

                    }
                    else
                    {
                        Response.StatusCode = 401;
                        st += "<table dir='ltr' border='1'>";
                        st += "<tr><th style='color:red'>Error</th></tr>";
                        st += "<tr><td colspan='2'>Username or password is incorrect</td></tr>";
                        st += "</table>";
                    }

                }

                resultLiteral.Text = st;
            }
            if ((Session["permission"] as bool? ?? false) == true)
            {
                Response.StatusCode = 401;
                st += "<p>You must log in first or <a href='/signup'>Sign Up</a></p>";
                Session["fromcreate"] = null;
                resultLiteral.Text = st;
            }
        }

    }
}