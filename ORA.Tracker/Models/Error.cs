using System.Text.Json;

namespace ORA.Tracker.Models
{
    public class Error
    {
        public static readonly string NotFound = new Error("Not Found").ToString();
        public static readonly string MethodNotAllowed = new Error("Method Not Allowed").ToString();
        public static readonly string UnknownError = new Error("Unknown Error").ToString();

        public string message { get; set; }
        public string documentation_url { get; set; }

        public Error(string message)
        {
            this.message = message;
            this.documentation_url = "https://ora.crabwave.com/documentation";
        }

        public override string ToString() => JsonSerializer.Serialize(this);
    }
}
