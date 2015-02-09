using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsClient
{
    public partial class UserInfoControl : UserControl
    {
        Client client;
        AgentInfo agentInfo;
        public string UserName { get { return lblUserName.Text; } set { lblUserName.Text = value; } }
        public UserInfoControl()
        {
            InitializeComponent();
        }
        public UserInfoControl(Client client,AgentInfo agentInfo)
        {
            this.client = client;
            this.agentInfo = agentInfo;
            InitializeComponent();
        }        

        private void UserInfoControl_DoubleClick(object sender, EventArgs e)
        {
            Chat chat = new Chat(this.client,this.agentInfo);            
            chat.Show();
        }
    }
}
