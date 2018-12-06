using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;

namespace CarpgLobby
{
    class Program
    {
        static void Main(string[] args)
        {
            LobbyService service = new LobbyService();

            if (Debugger.IsAttached || args.Contains("-console"))
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
