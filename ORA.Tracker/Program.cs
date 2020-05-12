using System;

using ORA.Tracker.Services;
using ORA.Tracker.Services.Databases;

namespace ORA.Tracker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Arguments arguments;
            try
            {
                arguments = Arguments.Parse(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            if (arguments.IsHelpRequested)
            {
                PrintProgramHelp();
                return;
            }

            ClusterManager.Instance.SetDatabase(new ClusterDatabase(arguments.ClusterDatabasePath));

            var tracker = new Tracker(arguments.Port);
            tracker.Start();
        }

        public static void PrintProgramHelp()
        {
            Console.WriteLine($@"Usage: ora-tracker [OPTIONS]
Run the Tracker program for project ORA.
By default the tracker is ran on port {Arguments.DefaultPort} and the database directory is '{Arguments.DefaultClusterDatabasePath}'.

Options:
  -p, --port        Specify the port that the tracker will listen to
  -d, --database    Specify the tracker database directory path
  -h, --help        Print this help message");
        }
    }
}
