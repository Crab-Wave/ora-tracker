using System.Text.Json;

namespace ORA.Tracker.Models
{
    public class Error
    {
        public string message { get; set; }
        public string documentation_url { get; set; }

        public Error(string message)
        {
            this.message = message;
            this.documentation_url = "https://ora.crabwave.com/documentation";
        }

        public override string ToString() => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });

        public byte[] ToBytes() => JsonSerializer.SerializeToUtf8Bytes(this, new JsonSerializerOptions { WriteIndented = true });
    }
}
