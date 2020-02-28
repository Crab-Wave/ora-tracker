using System.Text.Json;

namespace ORA.Tracker.Models
{
    public class Node
    {
        public string id { get; set; }
        public string current_ip { get; set; }

        public Node(string id, string current_ip)
        {
            this.id = id;
            this.current_ip = current_ip;
        }

        public override string ToString() => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
}
