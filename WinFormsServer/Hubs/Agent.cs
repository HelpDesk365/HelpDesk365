using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalRChat.Hubs
{
    class Agent
    {
        public string AgentId { get; set; }
        public string AgentName { get; set; }
        public List<Group> Groups { get; set; }
    }

    class Group 
    {
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public List<Client> Clients { get; set; }
        public bool IsConnected { get; set; }
    }

    class Client
    {
        public string ClientId { get; set; }
        public string ClientName { get; set; }
    }
}
