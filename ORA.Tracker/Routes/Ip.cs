using System.Net;
using System.Text;
using ORA.Tracker.Http;
using ORA.Tracker.Services;
using ORA.Tracker.Routes.Attributes;

namespace ORA.Tracker.Routes
{
    public class Ip : Route
    {
        public Ip(IServiceCollection services)
            : base(services)
        {
        }

        protected override void get(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
        {
            response.Close(Encoding.UTF8.GetBytes(request.Ip), true);
        }
    }
}
