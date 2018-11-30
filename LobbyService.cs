using CarpgLobby.Api;
using CarpgLobby.Properties;
using CarpgLobby.Provider;
using CarpgLobby.Utils;
using Microsoft.Owin.Hosting;
using System;
using System.ServiceProcess;
using System.Threading;

namespace CarpgLobby
{
    partial class LobbyService : ServiceBase
    {
        private Timer timer;
        private IDisposable webapi;

        public LobbyService()
        {
            InitializeComponent();
        }

        public void Start()
        {
            OnStart(new string[0]);
            Console.WriteLine("LobbyService is running. Press Q to stop...");
            while(true)
            {
                var key = Console.ReadKey(true);
                if (key.KeyChar == 'Q')
                    break;
            }
            OnStop();
        }

        protected override void OnStart(string[] args)
        {
            Logger.Init();
            Logger.Info("Service start.");
            Lobby.Instance = new Lobby();
            timer = new Timer(Callback, null, 10000, Timeout.Infinite);
            webapi = WebApp.Start<Startup>(Settings.Default.ApiUrl);
        }

        protected override void OnStop()
        {
            timer.Dispose();
            webapi.Dispose();
            Logger.Info("Service shutdown.");
        }

        private void Callback(object state)
        {
            Lobby.Instance.RefreshServers();
            timer.Change(10000, Timeout.Infinite);
        }
    }
}
