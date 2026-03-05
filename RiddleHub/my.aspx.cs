using System;
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
                Session["login_required"] = true;
                Response.Redirect("/login.aspx?return=my&required=1");
                return;
            }
            username.Text = Convert.ToString(Session["username"]) ?? string.Empty;
        }
    }
}
