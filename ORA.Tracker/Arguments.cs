using System;

namespace ORA.Tracker
{
    public class Arguments
    {
        public const int DefaultPort = 3000;
        public const string DefaultClusterDatabasePath = "./Database/Clusters";

        public int Port { get; }
        public string ClusterDatabasePath { get; }
        public bool IsHelpRequested { get; }

        public static Arguments Parse(string[] args)
        {
            int port = DefaultPort;
            string clusterDatabasePath = DefaultClusterDatabasePath;
            bool isHelpRequested = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-h" || args[i] == "--help")
                {
                    isHelpRequested = true;
                }
                else if (args[i] == "-p" || args[i] == "--port")
                {
                    if (++i >= args.Length)
                        throw new ArgumentException($"Missing parameter for '{args[i-1]}' option.");

                    if (!Int32.TryParse(args[i], out port))
                        throw new ArgumentException($"Invalid parameter for '{args[i-1]}' option.");
                }
                else if (args[i] == "-d" || args[i] == "--database")
                {
                    if (++i >= args.Length)
                        throw new ArgumentException($"Missing parameter for '{args[i-1]}' option.");

                    clusterDatabasePath = args[i];
                }
                else
                {
                    throw new ArgumentException($"Unknown option '{args[i]}'.");
                }
            }

            return new Arguments(port, clusterDatabasePath, isHelpRequested);
        }

        Arguments(int port, string clusterDatabasePath, bool isHelpRequested)
        {
            this.Port = port;
            this.ClusterDatabasePath = clusterDatabasePath;
            this.IsHelpRequested = isHelpRequested;
        }
    }
}
