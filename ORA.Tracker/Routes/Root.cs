using System.Net;
using System.Text;

namespace ORA.Tracker.Routes
{
    public class Root : Route
    {
        public Root()
            : base("/") { }

        protected override byte[] get(HttpListenerRequest request, HttpListenerResponse response)
            => Encoding.UTF8.GetBytes("Hey welcome to '/'");
    }
}
