using System.Net;

using ORA.Tracker.Services;
using ORA.Tracker.Http;

namespace ORA.Tracker.Routes
{
    public class Ping : Route
    {
        public Ping(ServiceCollection services)
            : base(services) { }

        protected override void post(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
            => response.Close(request.Body, true);
    }
}
