using System.Text.Json;

namespace ORA.Tracker.Models
{
    public class Node
    {
        public string uid { get; set; }
        public string current_ip { get; set; }

        public Node(string uid, string current_ip)
        {
            this.uid = uid;
            this.current_ip = current_ip;
        }

        public override string ToString() => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
}
