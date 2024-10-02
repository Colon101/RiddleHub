using System;

namespace RiddleHub
{
    public partial class success : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            signout.Text = "reload";
        }
    }
}