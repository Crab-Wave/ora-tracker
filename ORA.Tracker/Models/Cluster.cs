using System;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;

namespace ORA.Tracker.Models
{
    public class Cluster
    {
        public struct ClusterWithoutMemberName
        {
            public Guid id { get; set; }
            public string name { get; set; }
            public string owner { get; set; }
            public List<string> members { get; set; }
            public List<string> admins { get; set; }
            public List<string> files { get; set; }

            public ClusterWithoutMemberName(Cluster c)
            {
                this.id = c.id;
                this.name = c.name;
                this.owner = c.owner;
                this.members = c.members.Keys.ToList();
                this.admins = c.admins;
                this.files = c.files;
            }
        }

        private struct Id
        {
            public string id { get; set; }

            public Id(Guid id)
            {
                this.id = id.ToString();
            }
        }

        private struct Members
        {
            public List<Member> members { get; set; }

            public Members(Dictionary<string, string> members)
            {
                this.members = new List<Member>();
                foreach (var m in members)
                    this.members.Add(new Member(m.Key, m.Value));
            }
        }

        public Guid id { get; set; }
        public string name { get; set; }
        public string owner { get; set; }
        public Dictionary<string, string> members { get; set; }
        public List<string> admins { get; set; }
        public List<string> files { get; set; }

        public Cluster() { }

        public Cluster(Guid id, string name, string owner, string ownerName, Dictionary<string, string> members, List<string> admins, List<string> files)
        {
            this.id = id;
            this.name = name;
            this.owner = owner;
            this.members = members;
            this.admins = admins;
            this.files = files;

            if (!members.ContainsKey(owner))
                this.members.Add(owner, ownerName);
        }

        public Cluster(string name, string owner, string ownerName)
            : this(Guid.NewGuid(), name, owner, ownerName, new Dictionary<string, string>(), new List<string>(), new List<string>()) { }

        public byte[] Serialize() => JsonSerializer.SerializeToUtf8Bytes<Cluster>(this, new JsonSerializerOptions { WriteIndented = true });
        public static Cluster Deserialize(byte[] jsonBytes) => JsonSerializer.Deserialize<Cluster>(jsonBytes);

        public byte[] SerializeWithoutMemberName() =>
            JsonSerializer.SerializeToUtf8Bytes<ClusterWithoutMemberName>(new ClusterWithoutMemberName(this), new JsonSerializerOptions { WriteIndented = true });
        public byte[] SerializeId() => JsonSerializer.SerializeToUtf8Bytes<Id>(new Id(this.id), new JsonSerializerOptions { WriteIndented = true });
        public byte[] SerializeMembers() => JsonSerializer.SerializeToUtf8Bytes<Members>(new Members(this.members), new JsonSerializerOptions { WriteIndented = true });
        public byte[] SerializeAdmins() => JsonSerializer.SerializeToUtf8Bytes<List<string>>(this.admins, new JsonSerializerOptions { WriteIndented = true });
    }
}
