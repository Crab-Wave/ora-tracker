using System.Text.Json;

namespace ORA.Tracker.Models
{
    public class Error
    {
        public static readonly string NotFoundString = new Error("Not Found").ToString();
        public static readonly byte[] NotFoundBytes = new Error("Not Found").ToBytes();
        public static readonly string MethodNotAllowed = new Error("Method Not Allowed").ToString();
        public static readonly byte[] MethodNotAllowedBytes = new Error("Method Not Allowed").ToBytes();
        public static readonly string UnknownErrorString = new Error("Unknown Error").ToString();
        public static readonly byte[] UnknownErrorBytes = new Error("Unknown Error").ToBytes();

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
