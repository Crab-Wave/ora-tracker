using System;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;

using ORA.Tracker.Services.Managers;

namespace ORA.Tracker.Models
{
    public class Cluster
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public string owner { get; set; }
        public Dictionary<string, string> members { get; set; } // key: id, value: name
        public List<string> admins { get; set; }
        public List<File> files { get; set; }
        public List<string> invitedIdentities { get; set; }

        public Cluster() { }

        public Cluster(Guid id, string name, string owner, string ownerName, Dictionary<string, string> members, List<string> admins, List<File> files, List<string> invitedIdentities)
        {
            this.id = id;
            this.name = name;
            this.owner = owner;
            this.members = members;
            this.admins = admins;
            this.files = files;
            this.invitedIdentities = invitedIdentities;

            if (!members.ContainsKey(owner))
                this.members.Add(owner, ownerName);
        }

        public Cluster(string name, string owner, string ownerName)
            : this(Guid.NewGuid(), name, owner, ownerName, new Dictionary<string, string>(), new List<string>(), new List<File>(), new List<string>()) { }

        public bool IsOwnedBy(string id)
            => this.owner == id;

        public bool HasAdmin(string id)
            => this.admins.Contains(id);

        public bool HasMember(string id)
            => this.members.ContainsKey(id);

        public bool HasInvitedIdentity(string id)
            => this.invitedIdentities.Contains(id);

        public bool HasFile(string hash)
            => this.files.Exists(f => f.hash == hash);

        public File GetFile(string hash)
            => this.files.Find(f => f.hash == hash);

        public void AddFile(string userid, File file)
        {
            if (!file.HasOwner(userid))
                file.AddOwner(userid);
            if (!this.HasFile(file.hash))
                this.files.Add(file);
        }

        public void RemoveFile(string hash)
        {
            for (int i = 0; i < this.files.Count; i++)
            {
                if (this.files[i].hash == hash)
                {
                    this.files.RemoveAt(i);
                    return;
                }
            }
        }

        public List<Node> GetFileOwners(NodeManager nodeManager, string hash)
        {
            var fileOwners = new List<Node>();

            foreach (var id in this.members.Keys)
            {
                if (nodeManager.IsNodeOwningFile(id, hash))
                    fileOwners.Add(new Node(id, nodeManager.GetIp(id)));
            }

            return fileOwners;
        }

        public byte[] Serialize() => JsonSerializer.SerializeToUtf8Bytes<Cluster>(this, new JsonSerializerOptions { WriteIndented = true });
        public static Cluster Deserialize(byte[] jsonBytes) => JsonSerializer.Deserialize<Cluster>(jsonBytes);

        public byte[] SerializePublicInformation() =>
            JsonSerializer.SerializeToUtf8Bytes<PublicCluster>(new PublicCluster(this), new JsonSerializerOptions { WriteIndented = true });
        public byte[] SerializeId() => JsonSerializer.SerializeToUtf8Bytes<Id>(new Id(this.id), new JsonSerializerOptions { WriteIndented = true });
        public byte[] SerializeMembers() => JsonSerializer.SerializeToUtf8Bytes<Members>(new Members(this.members), new JsonSerializerOptions { WriteIndented = true });
        public byte[] SerializeAdmins() => JsonSerializer.SerializeToUtf8Bytes<List<string>>(this.admins, new JsonSerializerOptions { WriteIndented = true });

        public struct PublicCluster
        {
            public Guid id { get; set; }
            public string name { get; set; }
            public string owner { get; set; }
            public List<string> members { get; set; }
            public List<string> admins { get; set; }
            public List<string> files { get; set; }

            public PublicCluster(Cluster c)
            {
                this.id = c.id;
                this.name = c.name;
                this.owner = c.owner;
                this.members = c.members.Keys.ToList();
                this.admins = c.admins;
                this.files = c.files.Select(f => f.hash).ToList();
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
    }
}
