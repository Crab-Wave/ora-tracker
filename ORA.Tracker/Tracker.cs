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
            this.server.RegisterRoute("/auth", new Authentication(services));
            this.server.RegisterRoute("/refreshtoken", new RefreshToken(services));
            this.server.RegisterRoute("/ip", new Ip(services));
            this.server.RegisterRoute("/refreship", new RefreshIp(services));
            this.server.RegisterRoute("/disconnect", new Disconnect(services));
            this.server.RegisterRoute("/clusters/{id}", new Clusters(services));
            this.server.RegisterRoute("/clusters/{id}/join", new Join(services));
            this.server.RegisterRoute("/clusters/{id}/members", new Members(services));
            this.server.RegisterRoute("/clusters/{id}/admins", new Admins(services));
            this.server.RegisterRoute("/clusters/{id}/files", new Files(services));
            this.server.RegisterRoute("/clusters/{id}/files/owned", new FilesOwned(services));
        }

        public void Start() => this.server.Listen();

        public void Stop() => this.server.Stop();
    }
}
