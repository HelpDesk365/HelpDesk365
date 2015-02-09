using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

namespace WebClient
{
    public partial class CheckUserInfo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var userid = Request["userId"] ?? "";
            var pwd = Request["pwd"] ?? "";
            Response.Write(String.Format("{0}", Newtonsoft.Json.JsonConvert.SerializeObject(
                new
                {
                    UserId = userid,
                    UserName = "test"
                }
            )));
        }
    }
}