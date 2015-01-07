using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SignalRChat
{
    /// <summary>
    /// WinForms host for a SignalR server. The host can stop and start the SignalR
    /// server, report errors when trying to start the server on a URI where a
    /// server is already being hosted, and monitor when clients connect and disconnect. 
    /// The hub used in this server is a simple echo service, and has the same 
    /// functionality as the other hubs in the SignalR Getting Started tutorials.
    /// </summary>
    public partial class WinFormsServer : Form
    {
        private IDisposable SignalR { get; set; }
        const string ServerURI = "http://localhost:8080";

        internal WinFormsServer()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Calls the StartServer method with Task.Run to not
        /// block the UI thread. 
        /// </summary>
        private void ButtonStart_Click(object sender, EventArgs e)
        {
            WriteToConsole("Starting server...");
            ButtonStart.Enabled = false;
            Task.Run(() => StartServer());
        }

        /// <summary>
        /// Stops the server and closes the form. Restart functionality omitted
        /// for clarity.
        /// </summary>
        private void ButtonStop_Click(object sender, EventArgs e)
        {
            //SignalR will be disposed in the FormClosing event
            Close();
        }

        /// <summary>
        /// Starts the server and checks for error thrown when another server is already 
        /// running. This method is called asynchronously from Button_Start.
        /// </summary>
        private void StartServer()
        {
            try
            {
                SignalR = WebApp.Start(ServerURI);
            }
            catch (TargetInvocationException)
            {
                WriteToConsole("Server failed to start. A server is already running on " + ServerURI);
                //Re-enable button to let user try to start server again
                this.Invoke((Action)(() => ButtonStart.Enabled = true));
                return;
            }
            this.Invoke((Action)(() => ButtonStop.Enabled = true));
            WriteToConsole("Server started at " + ServerURI);
        }
        /// <summary>
        /// This method adds a line to the RichTextBoxConsole control, using Invoke if used
        /// from a SignalR hub thread rather than the UI thread.
        /// </summary>
        /// <param name="message"></param>
        internal void WriteToConsole(String message)
        {
            if (RichTextBoxConsole.InvokeRequired)
            {
                this.Invoke((Action)(() =>
                    WriteToConsole(message)
                ));
                return;
            }
            RichTextBoxConsole.AppendText(message + Environment.NewLine);
        }

        private void WinFormsServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            if (SignalR != null)
            {
                SignalR.Dispose();
            }
        }
    }
    /// <summary>
    /// Used by OWIN's startup process. 
    /// </summary>
    class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            var hubConfiguration = new HubConfiguration
            {
                // You can enable JSONP by uncommenting line below.
                // JSONP requests are insecure but some older browsers (and some
                // versions of IE) require JSONP to work cross domain
                 EnableJSONP = true
                 
            };

            app.MapSignalR(hubConfiguration);
        }
    }
    public class Group
    {
        public string Name { get; set; }
        public int MaxMember { get { return 2; } }
        public bool IsAvailable { get; set; }
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string GroupId { get; set; }
    }
    /// <summary>
    /// Echoes messages sent using the Send message by calling the
    /// addMessage method on the client. Also reports to the console
    /// when clients connect and disconnect.
    /// </summary>
    public class MyHub : Hub
    {
        public static List<Group> groupList = new List<Group>();
        public void Send(string name, string message,string groupName)
        {
            if (String.IsNullOrEmpty(groupName))
            {
                var group = groupList.FirstOrDefault(d => d.ClientId == Context.ConnectionId);
                groupName = group.Name;
            }

            Clients.Group(groupName).addMessage(name, message);
        }
        public override Task OnConnected()
        {
            Program.MainForm.WriteToConsole("Client connected: " + Context.ConnectionId);
            return base.OnConnected();
        }
        public override Task OnDisconnected()
        {
            var client = groupList.Where(d => d.ClientId == Context.ConnectionId).FirstOrDefault();
            var agent = groupList.Where(d => d.GroupId == Context.ConnectionId).FirstOrDefault();

            if (client != null)
            {
                client.IsAvailable = true;
                client.ClientId = String.Empty;
                Groups.Remove(Context.ConnectionId, client.Name);
                LeaveGroup(client.ClientName,client.Name);

            }

            if (agent != null)
            {
                groupList.Remove(agent);
                Groups.Remove(Context.ConnectionId,agent.Name);
            }

            Program.MainForm.WriteToConsole("Client disconnected: " + Context.ConnectionId);
            return base.OnDisconnected();
        }
        public void Join(string name, string groupName)
        {

            Clients.Group(groupName).joinMessage(name, String.Format("Join : {0}", name));
        }

        public void GetGroupList()
        {
            Clients.Caller.getGroupList(groupList.Select(d => d.Name).ToList());
        }

        public void SessionTransfer(string source, string target)
        {
            var sourceGroup = groupList.FirstOrDefault(d => d.Name == source);
            var clientId = String.Empty;
            if (sourceGroup != null)
            {
                var targetGroup = groupList.FirstOrDefault(d => d.Name == target);

                clientId = sourceGroup.ClientId;
                Groups.Remove(sourceGroup.ClientId, source);
                Groups.Add(sourceGroup.ClientId, target);
                Join(sourceGroup.ClientName, target);

                if (targetGroup != null)
                {
                    targetGroup.IsAvailable = false;
                    targetGroup.ClientId = clientId;
                    targetGroup.ClientName = sourceGroup.ClientName;
                }
                sourceGroup.IsAvailable = true;
                sourceGroup.ClientId = String.Empty;
                sourceGroup.ClientName = String.Empty;
            }
            Clients.Clients(new string[] { clientId }).changeGroupName(target);
        }
        public void FindGroup(string name)
        {
            var group = groupList.Where(d => d.IsAvailable).FirstOrDefault();
            if (group != null)
            {
                group.IsAvailable = false;
                group.ClientId = Context.ConnectionId;
                group.ClientName = name;
                Groups.Add(Context.ConnectionId, group.Name);
                
            }
            Clients.Caller.findedGroup(group == null ? "" : group.Name);
        }

        public Task JoinGroup(string name, string groupName)
        {
            var group = groupList.Where(d => d.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (group == null)
            {
                groupList.Add(new Group() { 
                 IsAvailable =true,
                  Name = groupName,
                  ClientId = String.Empty,
                 GroupId = Context.ConnectionId
                });
               
            }
            return Groups.Add(Context.ConnectionId, groupName);

        }

        public void LeaveGroup(string name,string groupName)
        {
            Clients.Group(groupName).leaveMessage(name);
        }
       
    }
}
