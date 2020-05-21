using ORA.Tracker.Routes;
using ORA.Tracker.Services;

namespace ORA.Tracker
{
    public class Tracker
    {
        private readonly Server server;

        public Tracker(Arguments arguments)
        {
            var services = ServiceCollectionBuilder.BuildFromArguments(arguments);

            this.server = new Server(arguments.Port);
            this.server.RegisterRoute("/", new Root(services));
            this.server.RegisterRoute("/ping", new Ping(services));
            this.server.RegisterRoute("/clusters/{id}", new Clusters(services));
            this.server.RegisterRoute("/auth", new Authentication(services));
            this.server.RegisterRoute("/clusters/{id}/members", new Members(services));
            this.server.RegisterRoute("/clusters/{id}/admins", new Admins(services));
        }

        public void Start() => this.server.Listen();

        public void Stop() => this.server.Stop();
    }
}
