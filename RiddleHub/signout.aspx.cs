using System;
using UtilFunctions;

namespace RiddleHub
{
    public partial class signout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            bool isLoggedIn = UtilFunctionsClass.isLoggedIn(Session);
            if (isLoggedIn)
            {
                UtilFunctionsClass.logOut(Session);
                Session["loggingOut"] = true;
                Response.Redirect("/success");
            }
        }
    }
}