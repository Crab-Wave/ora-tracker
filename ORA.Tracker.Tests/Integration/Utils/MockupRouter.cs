using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using ORA.Tracker.Routes;

namespace ORA.Tracker.Tests.Integration.Utils
{
    internal class MockupRouter
    {
        private HttpClient client = new HttpClient();
        private static int port = 15000;

        private string listenerUri;
        private HttpListener listener;
        private Router router;
        private Route route;

        public MockupRouter(string path, Route route)
        {
            this.listenerUri = $"http://localhost:{port++}";
            this.listener = new HttpListener() { Prefixes = { listenerUri + "/" } };
            this.router = new Router();
            this.router.RegisterRoute(path, route);
            this.route = route;

            if (!this.listener.IsListening)
                this.listener.Start();
        }

        public async Task<HttpResponseMessage> GetResponseOf(HttpMethod method, string path, byte[] body, string credentials)
        {
            var responseMessage = sendRequest(method, this.listenerUri + path, body, credentials);

            var context = await this.listener.GetContextAsync();
            this.router.HandleRequest(context);

            return await responseMessage;
        }

        public Task<HttpResponseMessage> GetResponseOf(HttpMethod method, string path, byte[] body)
            => this.GetResponseOf(method, path, body, null);

        public Task<HttpResponseMessage> GetResponseOf(HttpMethod method, string path)
            => this.GetResponseOf(method, path, null, null);

        private async Task<HttpResponseMessage> sendRequest(HttpMethod method, string uri, byte[] bodyContent, string credentials)
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
