using System.Collections.Generic;

namespace ORA.Tracker.Services.Managers
{
    public class NodeManager
    {
        private Dictionary<string, string> nodes;

        public NodeManager()
        {
            this.nodes = new Dictionary<string, string>();
        }

        public void Put(string id, string ip)
            => this.nodes[id] = ip;

        public string GetIp(string id)
            => this.nodes[id];
    }
}
