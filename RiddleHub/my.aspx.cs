using System;
using System.Diagnostics;
using UtilFunctions;

namespace RiddleHub
{
    public partial class My : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            bool loggedIn = UtilFunctionsClass.IsLoggedIn(Session);
            if (!loggedIn)
            {
                Session["permission"] = true;
                Response.Redirect("/login");
            }
            username.Text = Session["username"].ToString();
        }
    }
}