using Serilog;
using System;

namespace Fantome.Utilities
{
    public static class Logging
    {
        public const string LOGS_FOLDER = "Logs";

        public static void Initialize()
        {
            string logPath = string.Format(@"{0}\FantomeLog - {1}.txt", LOGS_FOLDER, DateTime.Now.ToString("dd.MM.yyyy - HH-mm-ss"));
            string loggingPattern = Config.Get<string>("LoggingPattern");

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(logPath, outputTemplate: loggingPattern)
                .CreateLogger();
        }
    }
}
