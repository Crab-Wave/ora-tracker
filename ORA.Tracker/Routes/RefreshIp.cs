using System.Net;
using System.Text;
using ORA.Tracker.Http;
using ORA.Tracker.Services;
using ORA.Tracker.Routes.Attributes;

namespace ORA.Tracker.Routes
{
    public class RefreshIp : Route
    {
        public RefreshIp(IServiceCollection services)
            : base(services)
        {
        }

        [Authenticate]
        protected override void post(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
        {
            string oldIp = Encoding.Default.GetString(request.Body);
            this.services.NodeManager.UpdateIp(oldIp, request.Ip);
            response.Close(Encoding.UTF8.GetBytes(request.Ip), true);
        }
    }
}
