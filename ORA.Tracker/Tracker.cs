using ORA.Tracker.Routes;

namespace ORA.Tracker
{
    public class Tracker
    {
        private readonly Server server;

        public Tracker(int port)
        {
            this.server = new Server(port);

            this.server.RegisterRoute("/", new Root());
            this.server.RegisterRoute("/clusters/{id}", new Clusters());
            this.server.RegisterRoute("/auth", new Authentication());
            this.server.RegisterRoute("/clusters/{id}/members", new Members());
            this.server.RegisterRoute("/clusters/{id}/admins", new Members());
        }

        public void Start() => this.server.Listen();

        public void Stop() => this.server.Stop();
    }
}
