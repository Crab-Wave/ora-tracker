using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ORA.Tracker.Tests.Utils
{
    internal class MockupListener
    {
        private HttpClient client;
        private string listener_uri;
        private HttpListener listener;

        public MockupListener(int port)
        {
            this.client = new HttpClient();
            this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("test");
            this.listener_uri = $"http://localhost:{port}/";
            this.listener = new HttpListener() { Prefixes = { listener_uri } };

            if (!this.listener.IsListening)
                this.listener.Start();
        }

        public async Task<HttpListenerContext> GenerateContext(string path, HttpMethod method)
        {
            _ = this.makeRequest(method, this.listener_uri + path);
            HttpListenerContext context = await this.listener.GetContextAsync();

            return context;
        }

        private async Task<HttpResponseMessage> makeRequest(HttpMethod method, string uri)
        {
            return await this.client.SendAsync(new HttpRequestMessage(method, uri));
        }
    }
}
