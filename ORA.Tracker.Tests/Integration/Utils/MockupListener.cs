using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ORA.Tracker.Tests.Integration.Utils
{
    internal class MockupListener
    {
        private HttpClient client;
        private string listener_uri;
        private HttpListener listener;

        public MockupListener(int port)
        {
            this.client = new HttpClient();
            this.listener_uri = $"http://localhost:{port}/";
            this.listener = new HttpListener() { Prefixes = { listener_uri } };

            if (!this.listener.IsListening)
                this.listener.Start();
        }

        public async Task<HttpListenerContext> GenerateContext(string path, HttpMethod method, byte[] bodyContent, string credentials)
        {
            _ = this.makeRequest(method, this.listener_uri + path, bodyContent, credentials);
            HttpListenerContext context = await this.listener.GetContextAsync();

            return context;
        }

        public async Task<HttpListenerContext> GenerateContext(string path, HttpMethod method) =>
            await GenerateContext(path, method, null, null);

        public async Task<HttpListenerContext> GenerateContext(string path, HttpMethod method, byte[] bodyContent) =>
            await GenerateContext(path, method, bodyContent, null);

        public async Task<HttpListenerContext> GenerateContext(string path, HttpMethod method, string credentials) =>
            await GenerateContext(path, method, null, credentials);

        private async Task<HttpResponseMessage> makeRequest(HttpMethod method, string uri, byte[] bodyContent, string credentials)
        {
            HttpRequestMessage message = new HttpRequestMessage(method, uri);

            if (bodyContent != null)
                message.Content = new ByteArrayContent(bodyContent);
            if (credentials != null)
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", credentials);

            return await this.client.SendAsync(message);
        }
    }
}
