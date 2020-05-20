using ORA.Tracker.Services.Databases;

namespace ORA.Tracker.Services
{
    public static class ServiceCollectionBuilder
    {
        public static ServiceCollection BuildFromArguments(Arguments arguments)
        {
            return new ServiceCollection(new ClusterDatabase(arguments.ClusterDatabasePath));
        }
    }
}
