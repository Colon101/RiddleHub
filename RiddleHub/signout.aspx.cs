using System;
using RiddleHub.Security;
using UtilFunctions;

namespace RiddleHub
{
    public partial class Signout : System.Web.UI.Page
    {
        public string CsrfTokenEncoded = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            CsrfTokenEncoded = CsrfProtection.GetEncodedToken(Session);
            if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            if (CsrfProtection.RejectInvalidPost(Request, Response, Session))
            {
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (UtilFunctionsClass.IsLoggedIn(Session))
            {
                UtilFunctionsClass.LogOut(Session);
                Response.Redirect("/success", false);
            }
            else
            {
                Response.Redirect("/home", false);
            }
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
