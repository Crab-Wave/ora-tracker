using System.Net;

using ORA.Tracker.Http;
using ORA.Tracker.Services;
using ORA.Tracker.Routes.Attributes;

namespace ORA.Tracker.Routes
{
    public class RefreshIp : Route
    {
        public RefreshIp(IServiceCollection services)
            : base(services) { }

        [Authenticate]
        protected override void post(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
        {
            this.services.TokenManager.UpdateIp(request.Token, request.Ip);
            response.Close();
        }
    }
}
