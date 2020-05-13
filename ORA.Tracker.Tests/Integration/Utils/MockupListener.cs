using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using ORA.Tracker.Http;

namespace ORA.Tracker.Tests.Integration.Utils
{
    internal class MockupListener
    {
        private static readonly HttpClient client = new HttpClient();
        private string listener_uri;
        private HttpListener listener;

        public MockupListener(int port)
        {
            this.listener_uri = $"http://localhost:{port}/";
            this.listener = new HttpListener() { Prefixes = { listener_uri } };

            if (!this.listener.IsListening)
                this.listener.Start();
        }

        public async Task<(HttpRequest, HttpListenerResponse)> GetContext(string path, Dictionary<string, string> urlParameters,
            HttpMethod method, byte[] bodyContent, string credentials)
        {
            _ = this.makeRequest(method, this.listener_uri + path, bodyContent, credentials);
            HttpListenerContext context = await this.listener.GetContextAsync();

            return (new HttpRequest(context.Request, urlParameters), context.Response);
        }

        public async Task<(HttpRequest, HttpListenerResponse)> GetContext(string path, HttpMethod method, byte[] bodyContent, string credentials)
        {
            _ = this.makeRequest(method, this.listener_uri + path, bodyContent, credentials);
            HttpListenerContext context = await this.listener.GetContextAsync();

            return (new HttpRequest(context.Request, new Dictionary<string, string>()), context.Response);
        }

        public async Task<(HttpRequest, HttpListenerResponse)> GetContext(string path, HttpMethod method) =>
            await GetContext(path, method, null, null);

        public async Task<(HttpRequest, HttpListenerResponse)> GetContext(string path, Dictionary<string, string> urlParameters, HttpMethod method) =>
            await GetContext(path, urlParameters, method, null, null);

        public async Task<(HttpRequest, HttpListenerResponse)> GetContext(string path, Dictionary<string, string> urlParameters, HttpMethod method, string credentials) =>
            await GetContext(path, urlParameters, method, null, credentials);

        public async Task<(HttpRequest, HttpListenerResponse)> GetContext(string path, HttpMethod method, byte[] bodyContent) =>
            await GetContext(path, method, bodyContent, null);

        public async Task<(HttpRequest, HttpListenerResponse)> GetContext(string path, HttpMethod method, string credentials) =>
            await GetContext(path, method, null, credentials);

        private async Task<HttpResponseMessage> makeRequest(HttpMethod method, string uri, byte[] bodyContent, string credentials)
        {
            HttpRequestMessage message = new HttpRequestMessage(method, uri);

            if (bodyContent != null)
                message.Content = new ByteArrayContent(bodyContent);
            if (credentials != null)
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", credentials);

            return await client.SendAsync(message);
        }
    }
}
