using System.Collections.Generic;
using ORA.Tracker.Models;

namespace ORA.Tracker.Services.Managers
{
    public class NodeManager
    {
        private Dictionary<string, Node> nodes;

        public NodeManager()
        {
            this.nodes = new Dictionary<string, Node>();
        }

        public void RegisterNode(Node node)
            => this.nodes[node.id] = node;

        public void UpdateIp(string id, string ip)
            => this.nodes[id].ip = ip;

        public void AddFilesOwned(string id, HashSet<string> files)
            => this.nodes[id].FilesOwned.UnionWith(files);

        public bool AddFileOwned(string id, string hash)
            => this.nodes[id].FilesOwned.Add(hash);

        public Node GetNode(string id)
            => this.nodes[id];

        public string GetIp(string id)
            => this.nodes[id].ip;

        public void Remove(string id)
            => this.nodes.Remove(id);

        public bool IsNodeRegistered(string id)
            => this.nodes.ContainsKey(id);

        public bool IsNodeOwningFile(string id, string hash)
            => this.nodes[id].FilesOwned.Contains(hash);
    }
}
