using CarpgLobby.Properties;
using Serilog;
using System;
using System.IO;

namespace CarpgLobby.Utils
{
    public class Logger
    {
        private static Serilog.Core.Logger log;
        private static int level;
        private static bool useConsole;
        public static int Errors;

        public static void Init(bool _useConsole)
        {
            useConsole = _useConsole;
            string dir = AppDomain.CurrentDomain.BaseDirectory + "logs";
            Directory.CreateDirectory(dir);
            switch (Settings.Default.LogLevel)
            {
                case "Verbose":
                    level = 0;
                    break;
                default:
                case "Information":
                    level = 1;
                    break;
                case "Warning":
                    level = 2;
                    break;
                case "Error":
                    level = 3;
                    break;
                case "Fatal":
                    level = 4;
                    break;
            }
            log = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.RollingFile(dir + "\\log-{Date}.txt")
                .CreateLogger();
        }

        public static void Verbose(string msg)
        {
            if (level == 0)
            {
                log.Write(Serilog.Events.LogEventLevel.Verbose, msg);
                if (useConsole)
                    Console.WriteLine($"[Verbose] {msg}");
            }
        }

        public static void Info(string msg)
        {
            if (level <= 1)
            {
                log.Write(Serilog.Events.LogEventLevel.Information, msg);
                if (useConsole)
                    Console.WriteLine($"[Info] {msg}");
            }
        }

        public static void Warning(string msg)
        {
            if (level <= 2)
            {
                log.Write(Serilog.Events.LogEventLevel.Warning, msg);
                if (useConsole)
                    Console.WriteLine($"[Warning] {msg}");
            }
        }

        public static void Error(string msg)
        {
            if (level <= 3)
            {
                log.Write(Serilog.Events.LogEventLevel.Error, msg);
                if (useConsole)
                    Console.WriteLine($"[Error] {msg}");
            }
            ++Errors;
        }
    }
}
