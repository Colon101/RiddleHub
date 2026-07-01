using System;

namespace RiddleHub
{
    public partial class connstring : System.Web.UI.Page
    {
        public string st;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Request.IsLocal)
            {
                Response.StatusCode = 404;
                connString.Text = string.Empty;
                return;
            }

            st += Server.HtmlEncode(Helper.GenerateConnectionString("db.mdf"));
            connString.Text = st;
        }
    }
}
