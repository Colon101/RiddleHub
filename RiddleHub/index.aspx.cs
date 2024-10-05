using System;
using UtilFunctions;

namespace RiddleHub
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        public string loginst;
        public string signupst;
        protected void Page_Load(object sender, EventArgs e)
        {
            bool loggedIn = UtilFunctionsClass.IsLoggedIn(Session);
            if (loggedIn)
            {
                loginText.Text = "Hi! " + Session["username"];
                signupText.Text = "Sign Out";

            }
            else
            {
                loginText.Text = "Login";
                signupText.Text = "Sign Up";
            }
        }
    }
}