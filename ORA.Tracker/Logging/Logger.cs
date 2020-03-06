using System;

namespace ORA.Tracker.Logging
{
    public class Logger
    {
        public enum Level
        {
            INFO,
            DEBUG,
            ERROR
        }

        private Action<string> loggingMethod;

        public Logger()
        {
            this.loggingMethod = Console.WriteLine;
        }

        public Logger(Action<string> loggingMethod)
        {
            this.loggingMethod = loggingMethod;
        }

        public void Log(Level level, string message)
        {
            switch (level)
            {
                case Level.INFO:    // Add an extra space after INFO to normalize output
                    this.loggingMethod($"{DateTime.Now.ToString()} [{level.ToString()} ] | {message}");
                    break;
                default:
                    this.loggingMethod($"{DateTime.Now.ToString()} [{level.ToString()}] | {message}");
                    break;
            }
        }

        public void Info(string message) => this.Log(Level.INFO, message);
        public void Info(Exception exception) => this.Log(Level.INFO, exception.Message);

        public void Debug(string message) => this.Log(Level.DEBUG, message);
        public void Debug(Exception exception) => this.Log(Level.DEBUG, exception.Message);

        public void Error(string message) => this.Log(Level.ERROR, message);
        public void Error(Exception exception) => this.Log(Level.ERROR, exception.Message);
    }
}
