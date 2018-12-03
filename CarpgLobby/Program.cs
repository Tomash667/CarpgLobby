using System.Diagnostics;
using System.ServiceProcess;

namespace CarpgLobby
{
    class Program
    {
        static void Main(string[] args)
        {
            LobbyService service = new LobbyService();

            if(Debugger.IsAttached)
            {
                service.Start();
            }
            else
            {
                ServiceBase.Run(service);
            }
        }
    }
}
