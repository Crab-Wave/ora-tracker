using System.Net;
using System.Text;
using ORA.Tracker.Http;
using ORA.Tracker.Services;
using ORA.Tracker.Routes.Attributes;

namespace ORA.Tracker.Routes
{
    public class RefreshToken : Route
    {
        public RefreshToken(IServiceCollection services)
            : base(services)
        {
        }

        [Authenticate]
        protected override void post(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
        {
            this.services.TokenManager.RefreshToken(request.Token);
            response.Close();
        }
    }
}
