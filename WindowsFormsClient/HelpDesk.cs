using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsClient
{
    public partial class HelpDesk : BaseForm
    {
        public class DataItem
        {
            public string Name { get; set; }
            [Browsable(false)]
            public string Category { get; set; }
        }
        private IHubProxy HubProxy { get; set; }
        const string ServerURI = "http://helpdesk.hunet.co.kr:8080/signalr";
        private HubConnection Connection { get; set; }
        public HelpDesk()
        {
            InitializeComponent();
            //grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            ConnectAsync();          
            
            this.Resize += HelpDesk_Resize;
            this.FormClosed += HelpDesk_FormClosed;
        }
        void BindData(List<Client> items)
        {
            panel.Controls.Clear();
            lblCount.Text = items.Count.ToString();
            foreach (var item in items)
            {
                panel.Controls.Add(new UserInfoControl(item,AgentInfo) { UserName = item.ClientName });
            }

        }
        void HelpDesk_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Connection != null)
            {
                Connection.Stop();
                Connection.Dispose();
            }           
        }

        void HelpDesk_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }
        private async void ConnectAsync()
        {
            Connection = new HubConnection(ServerURI);
            HubProxy = Connection.CreateHubProxy("MyHub");
            HubProxy.On<List<Client>>("JoinAgentList", (data) =>

              this.Invoke((Action)(() =>
              {
                  BindData(data);
                  //grid.DataSource = data;
              }
              ))
          );

            HubProxy.On<List<Client>>("GetWaittingClients", (data) =>

              this.Invoke((Action)(() =>
              {
                  BindData(data);
                  //grid.DataSource = data;
              }
              ))
          );


            
            try
            {
                await Connection.Start().ContinueWith(d =>
                {
                    HubProxy.Invoke("JoinAgentList", "",AgentInfo.UserId);
                });

            }
            catch (HttpRequestException)
            {
                return;
            }
        }     
        

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show("로그아웃 하시겠습니까?","LogOut",MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
            {
                base.Dispose(true);
                Application.Exit();
            }
            else
            {
                // If 'No', do something here.
            }
        }

        private void btnList_Click(object sender, EventArgs e)
        {
            DisableImage();
            btnList.BackgroundImage = (System.Drawing.Image)ResourceUtil.btnList_BackgroundImage;
        }

        private void btnMemo_Click(object sender, EventArgs e)
        {
            DisableImage();
            btnMemo.BackgroundImage = (System.Drawing.Image)ResourceUtil.footmn03_on;
        }

        private void btnSetting_Click(object sender, EventArgs e)
        {
            DisableImage();
            btnSetting.BackgroundImage = (System.Drawing.Image)ResourceUtil.footmn04_on;
        }

        void DisableImage()
        {
            btnLogOut.BackgroundImage = (System.Drawing.Image)ResourceUtil.btnLogOut_BackgroundImage;
            btnList.BackgroundImage = (System.Drawing.Image)ResourceUtil.footmn02_off; ;
            btnMemo.BackgroundImage = (System.Drawing.Image)ResourceUtil.btnMemo_BackgroundImage;
            btnSetting.BackgroundImage = (System.Drawing.Image)ResourceUtil.btnSetting_BackgroundImage;
        }

        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }
    }
}
