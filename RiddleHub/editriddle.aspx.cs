using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RiddleHub
{
    public partial class editriddle : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if(Request.RequestType == "GET")
            {
                if (string.IsNullOrEmpty(Request.Params["id"]))
                {
                    Response.StatusCode = 400;
                    Response.Redirect("/my");
                }
                string id = (string)Request.Params["id"];
                riddleInfo.Text += id.ToString();
                
            }
            if (Request.RequestType == "POST")
            {
                riddleInfo.Text += "post request";
            }
        }
    }
}