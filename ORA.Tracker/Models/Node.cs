using System.Collections.Generic;

namespace ORA.Tracker.Models
{
    public class Node
    {
        public string id { get; set; }
        public string ip { get; set; }
        public HashSet<string> FilesOwned { get; set; }

        public Node(string id, string ip, HashSet<string> filesOwned)
        {
            this.id = id;
            this.ip = ip;
            this.FilesOwned = filesOwned;
        }

        public Node(string id, string ip)
            : this(id, ip, new HashSet<string>())
        {
        }

        public bool DoesOwnFile(string hash)
            => this.FilesOwned.Contains(hash);
    }
}
