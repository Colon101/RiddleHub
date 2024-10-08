using System;
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
                Debug.WriteLine(Request.Form["submit"] + Request.Form["riddle"] + Request.Form["hint"] + Request.Form["answer"]);

            }
        }
    }
}