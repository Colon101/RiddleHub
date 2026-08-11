using System;
using RiddleHub.Security;
using UtilFunctions;

namespace RiddleHub
{
    public partial class My : System.Web.UI.Page
    {
        public string CsrfTokenJavaScript = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!UtilFunctionsClass.IsLoggedIn(Session))
            {
                Session["login_required"] = true;
                Response.Redirect("/login.aspx?return=my&required=1");
                return;
            }
            username.Text = System.Web.HttpUtility.HtmlEncode(Convert.ToString(Session["username"]) ?? string.Empty);
            CsrfTokenJavaScript = System.Web.HttpUtility.JavaScriptStringEncode(
                CsrfProtection.GetToken(Session));
        }
    }
}
