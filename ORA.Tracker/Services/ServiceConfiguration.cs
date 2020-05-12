using ORA.Tracker.Services.Managers;
using ORA.Tracker.Services.Databases;

namespace ORA.Tracker.Services
{
    public class ServiceConfiguration
    {
        private ClusterManager clusterManager;
        private TokenManager tokenManager;

        public ClusterManager ClusterManager { get => clusterManager; }
        public TokenManager TokenManager { get => tokenManager; }

        public ServiceConfiguration(Arguments arguments)
        {
            tokenManager = new TokenManager();
            clusterManager = new ClusterManager(new ClusterDatabase(arguments.ClusterDatabasePath));
        }
    }
}
