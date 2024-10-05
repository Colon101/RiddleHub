using System;
using UtilFunctions;

namespace RiddleHub
{
    public partial class Signout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            bool isLoggedIn = UtilFunctionsClass.IsLoggedIn(Session);
            if (isLoggedIn)
            {
                UtilFunctionsClass.LogOut(Session);
                Session["loggingOut"] = true;
                Response.Redirect("/success");
            }
            else
            {
                Response.Redirect("/home");
            }
        }
    }
}