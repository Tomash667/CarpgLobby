using CarpgLobby.Api;
using CarpgLobby.Properties;
using CarpgLobby.Provider;
using CarpgLobby.Proxy;
using CarpgLobby.Utils;
using Microsoft.Owin.Hosting;
using System;
using System.Linq;
using System.ServiceProcess;
using System.Timers;

namespace CarpgLobby
{
    partial class LobbyService : ServiceBase
    {
        private IDisposable webapi;
        private SLikeNetProxy proxy = new SLikeNetProxy();
        private Timer timer;

        public LobbyService()
        {
            InitializeComponent();
        }

        public void Start()
        {
            OnStart(new string[] { "console" });
            Console.WriteLine("LobbyService is running. Press Q to stop...");
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.KeyChar == 'Q')
                    break;
            }
            OnStop();
        }

        protected override void OnStart(string[] args)
        {
            Logger.Init(args.Contains("console"));
            Logger.Info("Service start.");
            Lobby.Instance = new Lobby();
            proxy.Init();
            Logger.Info($"Current version: {Lobby.Instance.VersionStr}");
            webapi = WebApp.Start<Startup>(Settings.Default.ApiUrl);

            timer = new Timer();
            timer.Interval = new TimeSpan(1, 0, 0).TotalMilliseconds;
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            timer.Start();
        }

        protected override void OnStop()
        {
            timer.Stop();
            webapi.Dispose();
            proxy.Shutdown();
            Logger.Info("Service shutdown.");
        }

        protected override void OnShutdown()
        {
            OnStop();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DateTime now = DateTime.Now;
            if (now.DayOfWeek == DayOfWeek.Monday && now.Day < 7 && now.Hour == 5)
                proxy.Restart();
        }
    }
}
