using System;
using System.Collections.Generic;
using System.Linq;
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

        public Node GetNode(string ip) => this.nodes.GetValueOrDefault(ip, null);

        public void RegisterNode(Node node)
            => this.nodes[node.ip] = node;

        public void UpdateIp(string oldIp, string newIp)
        {
            Node node = this.nodes[oldIp];
            node.id = newIp;
            this.Remove(oldIp);
            this.RegisterNode(node);
        }

        public void AddFilesOwned(string ip, HashSet<string> files)
            => this.nodes[ip].FilesOwned.UnionWith(files);

        public bool AddFileOwned(string ip, string hash)
            => this.nodes[ip].FilesOwned.Add(hash);

        public Node[] GetNodes(string id)
            => this.nodes.Values.Where(node => node.id.Equals(id)).ToArray();

        public void Remove(string ip)
            => this.nodes.Remove(ip);

        public bool IsNodeRegistered(string ip)
            => this.nodes.ContainsKey(ip);
    }
}
