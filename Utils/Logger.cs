using Serilog;
using System;
using System.IO;

namespace CarpgLobby.Utils
{
    public class Logger
    {
        private static Serilog.Core.Logger log;

        public static void Init()
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory + "logs";
            Directory.CreateDirectory(dir);
            log = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.RollingFile(dir + "\\log-{Date}.txt")
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
        }
    }
}
