using System.Net.Http;
using System.Net.Http.Headers;

namespace ORA.Tracker.Tests.Integration.Utils
{
    internal class MockupRouterRequest
    {
        public HttpMethod Method { get; }
        public string Path { get; }
        public byte[] Body { get; set; }
        public string CredentialsType { get; set; }
        public string Credentials { get; set; }
        public AuthenticationHeaderValue Authorization
        {
            get
            {
                if (this.Credentials != null)
                    return new AuthenticationHeaderValue(CredentialsType, Credentials);
                return null;
            }
        }

        public MockupRouterRequest(HttpMethod method, string path)
        {
            this.Method = method;
            this.Path = path;
            this.Body = null;
            this.CredentialsType = "Bearer";
            this.Credentials = null;
        }
    }
}
