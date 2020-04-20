using System;

namespace ORA.Tracker
{
    class Arguments
    {
        public int Port { get; }
        public string DatabasePath { get; }
        public bool IsHelpRequested { get; }

        public static Arguments Parse(string[] args)
        {
            // Default values for argument fields
            int port = 3000;
            string databasePath = "./Database";
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
                        throw new Exception($"Missing argument for '{args[i-1]}' option.");

                    if (!Int32.TryParse(args[i], out port))
                        throw new Exception($"Invalid argument for '{args[i-1]}' option.");
                }
                else if (args[i] == "-d" || args[i] == "--database")
                {
                    if (++i >= args.Length)
                        throw new Exception($"Missing argument for '{args[i-1]}' option.");

                    databasePath = args[i];
                }
                else
                {
                    throw new Exception($"Unknown option '{args[i]}'");
                }
            }

            return new Arguments(port, databasePath, isHelpRequested);
        }

        Arguments(int port, string databasePath, bool isHelpRequested)
        {
            this.Port = port;
            this.DatabasePath = databasePath;
            this.IsHelpRequested = isHelpRequested;
        }
    }
}
