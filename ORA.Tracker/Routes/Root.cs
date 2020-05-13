using System.Net;
using System.Text;

using ORA.Tracker.Http;
using ORA.Tracker.Services;

namespace ORA.Tracker.Routes
{
    public class Root : Route
    {
        private static readonly byte[] welcomeMessage = Encoding.UTF8.GetBytes("Hey welcome to '/'");

        public Root(IServiceCollection services)
            : base(services) { }

        protected override byte[] get(HttpRequest request, HttpListenerResponse response)
            => welcomeMessage;
    }
}
