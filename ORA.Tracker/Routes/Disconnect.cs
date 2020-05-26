using System.Net;
using ORA.Tracker.Http;
using ORA.Tracker.Services;
using ORA.Tracker.Routes.Attributes;

namespace ORA.Tracker.Routes
{
    public class Disconnect : Route
    {
        public Disconnect(IServiceCollection services)
            : base(services)
        {
        }

        [Authenticate]
        protected override void delete(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
        {
            this.services.TokenManager.RemoveNode(this.services.TokenManager.GetIdFromIp(request.Ip), request.Ip);

            response.Close();
        }
    }
}
