using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalRChat.Hubs
{
    class ChatHub : Hub
    {
        public static List<Agent> agentList = new List<Agent>();
        public void Send(string name, string agentId, string message, string groupId)
        {
            Clients.Group(groupId).addMessage(name, message);
        }
        public override Task OnConnected()
        {
            Program.MainForm.WriteToConsole("Client connected: " + Context.ConnectionId);
            return base.OnConnected();
        }
       
        public override Task OnDisconnected(bool isStopCall)
        {
            var agent = agentList.FirstOrDefault(d => d.Groups.Count(o => o.GroupId == Context.ConnectionId) > 0);
            if (agent != null)
            {
                var group  = agent.Groups.FirstOrDefault(d=>d.GroupId == Context.ConnectionId);                
                foreach (var item in group.Clients)
                {
                    Groups.Remove(item.ClientId, group.GroupId);
                }
                agent.Groups.Remove(group);
            }

            Program.MainForm.WriteToConsole("Client disconnected: " + Context.ConnectionId);
            return base.OnDisconnected(isStopCall);
        }        

       

    }
}
