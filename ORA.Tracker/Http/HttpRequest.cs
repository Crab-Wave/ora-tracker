using System.Net;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace ORA.Tracker.Http
{
    public class HttpRequest
    {
        private HttpListenerRequest request;
        private Dictionary<string, string> urlParameters;
        private byte[] body;
        private string token;

        public bool HasEntityBody { get => this.request.HasEntityBody; }
        public NameValueCollection Headers { get => this.request.Headers; }
        public string HttpMethod { get => this.request.HttpMethod; }
        public Stream InputStream { get => this.request.InputStream; }
        public NameValueCollection QueryString { get => this.request.QueryString; }
        public Dictionary<string, string> UrlParameters { get => urlParameters; }
        public byte[] Body { get => this.body ?? (this.body = this.getBody()); }
        public string Token { get => this.token ?? (this.token = Services.Authorization.GetToken(this.request.Headers)); }

        public HttpRequest(HttpListenerRequest request, Dictionary<string, string> urlParameters)
        {
            this.request = request;
            this.urlParameters = urlParameters;
            this.body = null;
            this.token = null;
        }

        private byte[] getBody()
        {
            Stream body = this.request.InputStream;
            var ms = new MemoryStream(512);
            body.CopyTo(ms);

            return ms.ToArray();
        }
    }
}
