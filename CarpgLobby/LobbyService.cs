using CarpgLobby.Api;
using CarpgLobby.Properties;
using CarpgLobby.Provider;
using CarpgLobby.Proxy;
using CarpgLobby.Utils;
using Microsoft.Owin.Hosting;
using System;
using System.Linq;
using System.ServiceProcess;

namespace CarpgLobby
{
    partial class LobbyService : ServiceBase
    {
        private IDisposable webapi;
        private SLikeNetProxy proxy = new SLikeNetProxy();

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
            Logger.Info($"Current version: {Utils.Version.Current}");
            webapi = WebApp.Start<Startup>(Settings.Default.ApiUrl);
        }

        protected override void OnStop()
        {
            webapi.Dispose();
            proxy.Shutdown();
            Logger.Info("Service shutdown.");
        }

        protected override void OnShutdown()
        {
            OnStop();
        }
    }
}
