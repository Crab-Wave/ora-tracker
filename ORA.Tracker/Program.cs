using System;

namespace ORA.Tracker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // TODO: port with console arguments

            var tracker = new Tracker(3000);
            tracker.Start();
        }
    }
}
