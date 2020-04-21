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

        public MockupListener(int port, AuthenticationHeaderValue authorization = null)
        {
            this.client = new HttpClient();
            if (authorization != null)
                this.client.DefaultRequestHeaders.Authorization = authorization;
            this.listener_uri = $"http://localhost:{port}/";
            this.listener = new HttpListener() { Prefixes = { listener_uri } };

            if (!this.listener.IsListening)
                this.listener.Start();
        }

        public MockupListener(int port, string credentials)
            : this(port, new AuthenticationHeaderValue("Bearer", credentials)) { }

        public async Task<HttpListenerContext> GenerateContext(string path, HttpMethod method, byte[] bodyContent = null)
        {
            _ = this.makeRequest(method, this.listener_uri + path, bodyContent);
            HttpListenerContext context = await this.listener.GetContextAsync();

            return context;
        }

        private async Task<HttpResponseMessage> makeRequest(HttpMethod method, string uri, byte[] bodyContent)
        {
            HttpRequestMessage message;
            if (bodyContent != null)
                message = new HttpRequestMessage(method, uri) { Content = new ByteArrayContent(bodyContent) };
            else
                message = new HttpRequestMessage(method, uri);

            return await this.client.SendAsync(message);
        }
    }
}
