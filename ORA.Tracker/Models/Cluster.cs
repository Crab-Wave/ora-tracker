using System;
using System.Text.Json;
using System.Collections.Generic;

namespace ORA.Tracker.Models
{
    public class Cluster
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public List<string> members { get; set; }
        public List<string> admins { get; set; }
        public List<string> files { get; set; }

        public Cluster(string name)
            : this(new Guid(), name, new List<string>(), new List<string>(), new List<string>()) { }

        public Cluster(Guid id, string name, List<string> members, List<string> admins, List<string> files)
        {
            this.id = id;
            this.name = name;
            this.members = members;
            this.admins = admins;
            this.files = files;
        }

        public override string ToString() => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
}
