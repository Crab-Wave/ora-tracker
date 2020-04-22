using System;

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

            ClusterDatabase.Init(arguments.ClusterDatabasePath);

            var tracker = new Tracker(arguments.Port);
            tracker.Start();
        }

        public static void PrintProgramHelp()
        {
            Console.WriteLine(@"Usage: ora-tracker [ARGUMENTS]
Run the Tracker program for project ORA.
By default the tracker is ran on port 3000 and the database directory is './Database'.

Arguments:
  -p, --port      Specify the port that the tracker will listen to
  -d, --database  Specify the tracker database directory path
  -h, --help      Print this help message");
        }
    }
}
