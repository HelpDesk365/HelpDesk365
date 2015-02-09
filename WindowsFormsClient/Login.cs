using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace WinFormsClient
{
    public partial class Login : BaseForm
    {
        
        public Login()
        {
            InitializeComponent();            
        }

       
        private void button1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(txtId.Text.Trim()))
            {
                MessageBox.Show("ID를 입력해 주세요.");
                return;
            }
            if (String.IsNullOrEmpty(txtId.Text.Trim()))
            {
                MessageBox.Show("패스워드를 입력해 주세요.");
                return;
            }

            var request = (HttpWebRequest)WebRequest.Create("http://helpdesk.hunet.co.kr/CheckUserInfo.aspx");
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                //string json = Newtonsoft.Json.JsonConvert.SerializeObject(new
                //{
                //    userId = txtId.Text.Trim(),
                //    pwd = txtPwd.Text.Trim()
                //});
                string paramter = String.Format("userId={0}&pwd={1}", txtId.Text, txtPwd.Text);

                streamWriter.Write(paramter);
            }

            var response = (HttpWebResponse)request.GetResponse();
            string result = String.Empty;
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }
            var userInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<AgentInfo>(result);
            HelpDesk helpDesk = new HelpDesk();
            helpDesk.source = this;
            helpDesk.AgentInfo = userInfo;
            helpDesk.Show();            
            this.Dispose(false);
           
        }

       
        private void txtId_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (Char)Keys.Return)
            {
                button1_Click(sender, e);
            }
        }

        private void txtPwd_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (Char)Keys.Return)
            {
                button1_Click(sender, e);
            }
        }
    }
}
