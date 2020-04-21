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
            this.server.RegisterRoute("/clusters", new Clusters());
            this.server.RegisterRoute("/token", new Token());
            this.server.RegisterRoute("/auth", new Authentication());
        }

        public void Start() => this.server.Listen();

        public void Stop() => this.server.Stop();
    }
}
