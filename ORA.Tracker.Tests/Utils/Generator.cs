using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ORA.Tracker.Tests.Utils
{
    internal class Generator
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string LISTENER_URI = "http://localhost:15181";
        private static readonly HttpListener listener = new HttpListener() { Prefixes = { "http://localhost:15181/" } };

        public static async Task<HttpListenerContext> GenerateListenerContext(string path, HttpMethod method)
        {
            if (!listener.IsListening)
                listener.Start();

            _ = MakeRequest(method, LISTENER_URI + path);
            HttpListenerContext context = await listener.GetContextAsync();

            return context;
        }

        public static async Task<HttpListenerContext> GenerateListenerContext(string path,
                                                                              HttpMethod method, HttpContent content)
        {
            if (!listener.IsListening)
                listener.Start();

            _ = MakeRequest(method, LISTENER_URI + path, content);
            HttpListenerContext context = await listener.GetContextAsync();

            return context;
        }

        private static async Task<HttpResponseMessage> MakeRequest(HttpMethod method, string uri)
        {
            return await client.SendAsync(new HttpRequestMessage(method, uri));
        }

        private static async Task<HttpResponseMessage> MakeRequest(HttpMethod method, string uri, HttpContent content)
        {
            return await client.SendAsync(new HttpRequestMessage(method, uri) { Content = content });
        }
    }
}
