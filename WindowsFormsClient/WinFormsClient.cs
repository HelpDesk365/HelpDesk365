using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows.Forms;

namespace WinFormsClient
{
    /// <summary>
    /// SignalR client hosted in a WinForms application. The client
    /// lets the user pick a user name, connect to the server asynchronously
    /// to not block the UI thread, and send chat messages to all connected 
    /// clients whether they are hosted in WinForms, WPF, or a web application.
    /// </summary>
    public partial class WinFormsClient : Form
    {
        /// <summary>
        /// This name is simply added to sent messages to identify the user; this 
        /// sample does not include authentication.
        /// </summary>
        private String UserName { get; set; }
        private IHubProxy HubProxy { get; set; }
        const string ServerURI = "http://helpdesk.hunet.co.kr:8080/signalr";
        private HubConnection Connection { get; set; }
        
        internal WinFormsClient()
        {
            InitializeComponent();
        }

        private void ButtonSend_Click(object sender, EventArgs e)
        {
            HubProxy.Invoke("Send", UserName, TextBoxMessage.Text, UserName);
            TextBoxMessage.Text = String.Empty;
            TextBoxMessage.Focus();
        }

        private void Form_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        /// <summary>
        /// Creates and connects the hub connection and hub proxy. This method
        /// is called asynchronously from SignInButton_Click.
        /// </summary>
        private async void ConnectAsync()
        {
            Connection = new HubConnection(ServerURI);
            Connection.Closed += Connection_Closed;
            HubProxy = Connection.CreateHubProxy("MyHub");
            //Handle incoming event from server: use Invoke to write to console from SignalR's thread
            HubProxy.On<string, string>("AddMessage", (name, message) =>
                
                this.Invoke((Action)(() =>{
                    RichTextBoxConsole.AppendText(String.Format("{0}: {1}" + Environment.NewLine, name, message));
                    if (this.WindowState ==  FormWindowState.Minimized)
                    {
                        notifyIcon1.BalloonTipText = message;
                        notifyIcon1.BalloonTipClicked += notifyIcon1_BalloonTipClicked;
                        notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
                        notifyIcon1.BalloonTipTitle = String.Format("from{0}",name);
                        notifyIcon1.ShowBalloonTip(500);
                    }
                    
                }
                ))
            );
            HubProxy.On<string>("SessionTranserMessage", (message) => 
                this.Invoke((Action)(() =>
                {
                    MessageBox.Show(message);
                }
                ))
            );
            HubProxy.On<string>("JoinMessage", (name) =>
                
                this.Invoke((Action)(() =>{
                    RichTextBoxConsole.AppendText(String.Format("{0}님이 입장하셨습니다." + Environment.NewLine, name));
                }
                ))
            );

            HubProxy.On<string>("LeaveMessage", (name) =>

               this.Invoke((Action)(() =>
               {
                   RichTextBoxConsole.AppendText(String.Format("{0}님이 퇴장하셨습니다." + Environment.NewLine, name));
               }
               ))
           );

            HubProxy.On<List<string>>("GetGroupList", (data) =>

               this.Invoke((Action)(() =>
               {
                   comboBox1.DataSource = data;                 
               }
               ))
           );
            

            try
            {
                await Connection.Start().ContinueWith(d => {
                    HubProxy.Invoke("CreateGroup", UserName, UserName,"");
                });
                
            }
            catch (HttpRequestException)
            {
                StatusText.Text = "Unable to connect to server: Start server before connecting clients.";
                //No connection: Don't enable Send button or show chat UI
                return;
            }

            //Activate UI
            SignInPanel.Visible = false;
            ChatPanel.Visible = true;
            ButtonSend.Enabled = true;
            TextBoxMessage.Focus();
            RichTextBoxConsole.AppendText("Connected to server at " + ServerURI + Environment.NewLine);
        }

        void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        /// <summary>
        /// If the server is stopped, the connection will time out after 30 seconds (default), and the 
        /// Closed event will fire.
        /// </summary>
        private void Connection_Closed()
        {
            //Deactivate chat UI; show login UI. 
            this.Invoke((Action)(() => ChatPanel.Visible = false));
            this.Invoke((Action)(() => ButtonSend.Enabled = false));
            this.Invoke((Action)(() => StatusText.Text = "You have been disconnected."));
            this.Invoke((Action)(() => SignInPanel.Visible = true));
        }

        private void SignInButton_Click(object sender, EventArgs e)
        {
            UserName = UserNameTextBox.Text;
            //Connect to server (use async method to avoid blocking UI thread)
            if (!String.IsNullOrEmpty(UserName))
            {
                StatusText.Visible = true;
                StatusText.Text = "Connecting to server...";
                ConnectAsync();
            }
        }

        private void WinFormsClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Connection != null)
            {
                Connection.Stop();
                Connection.Dispose();
            }
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void btnSessionTransfer_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("상담사 아이디를 선택해 주세요.");
            }
            HubProxy.Invoke("SessionTransfer", UserName, comboBox1.SelectedItem.ToString());
        }

        private void comboBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            HubProxy.Invoke("GetGroupList");
        }

        private void comboBox1_MouseClick(object sender, MouseEventArgs e)
        {
            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_Click(object sender, EventArgs e)
        {
            HubProxy.Invoke("GetGroupList");
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            HubProxy.Invoke("DisconnectClient", UserName);
        }
    }
}
