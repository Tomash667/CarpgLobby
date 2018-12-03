using CarpgLobby.Properties;
using System.Runtime.InteropServices;

namespace CarpgLobby.Utils
{
    public class SLikeNetProxy
    {
        public delegate void Callback([MarshalAs(UnmanagedType.LPStr)]string msg);

        [DllImport("SLikeNetProxy.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool InitProxy(int players, int port, [MarshalAs(UnmanagedType.FunctionPtr)]Callback callback);

        [DllImport("SLikeNetProxy.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void ShutdownProxy();

        public static void Init()
        {
            if (!InitProxy(Settings.Default.ProxyPlayers, Settings.Default.ProxyPort, str => Logger.Info(str)))
                throw new System.Exception("Failed to init proxy.");
        }

        public static void Shutdown()
        {
            ShutdownProxy();
        }
    }
}
