using System.Text.Json;

namespace ORA.Tracker.Models
{
    public class Cluster
    {
        public string id { get; set; }
        public string name { get; set; }
        public string[] members { get; set; }
        public string[] admins { get; set; }
        public string[] files { get; set; }

        public Cluster(string id, string name, string[] members, string[] admins, string[] files)
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
