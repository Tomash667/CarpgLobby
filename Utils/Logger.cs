using CarpgLobby.Properties;
using Serilog;
using System;
using System.IO;

namespace CarpgLobby.Utils
{
    public class Logger
    {
        private static Serilog.Core.Logger log;
        public static int Errors;

        public static void Init()
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory + "logs";
            Directory.CreateDirectory(dir);
            var config = new LoggerConfiguration();
            switch (Settings.Default.LogLevel)
            {
                case "Verbose":
                    config = config.MinimumLevel.Verbose();
                    break;
                default:
                case "Information":
                    config = config.MinimumLevel.Information();
                    break;
                case "Error":
                    config = config.MinimumLevel.Error();
                    break;
                case "Fatal":
                    config = config.MinimumLevel.Fatal();
                    break;
            }
            log = config.WriteTo.RollingFile(dir + "\\log-{Date}.txt")
                .CreateLogger();
        }

        public static void Verbose(string msg)
        {
            log.Write(Serilog.Events.LogEventLevel.Verbose, msg);
        }

        public static void Info(string msg)
        {
            log.Write(Serilog.Events.LogEventLevel.Information, msg);
        }

        public static void Error(string msg)
        {
            log.Write(Serilog.Events.LogEventLevel.Error, msg);
            ++Errors;
        }
    }
}
