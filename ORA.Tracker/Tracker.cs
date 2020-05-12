using ORA.Tracker.Routes;
using ORA.Tracker.Services;

namespace ORA.Tracker
{
    public class Tracker
    {
        private readonly Server server;

        public Tracker(Arguments arguments)
        {
            var serviceConfiguration = new ServiceConfiguration(arguments);

            this.server = new Server(arguments.Port);
            this.server.RegisterRoute("/", new Root(serviceConfiguration));
            this.server.RegisterRoute("/clusters/{id}", new Clusters(serviceConfiguration));
            this.server.RegisterRoute("/auth", new Authentication(serviceConfiguration));
            this.server.RegisterRoute("/clusters/{id}/members", new Members(serviceConfiguration));
            this.server.RegisterRoute("/clusters/{id}/admins", new Admins(serviceConfiguration));
        }

        public void Start() => this.server.Listen();

        public void Stop() => this.server.Stop();
    }
}
