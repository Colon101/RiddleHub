using System;

namespace RiddleHub
{
    public partial class connstring : System.Web.UI.Page
    {
        public string st;
        protected void Page_Load(object sender, EventArgs e)
        {
            st += Helper.GenerateConnectionString("db.mdf");
            connString.Text = st;
        }
    }
}