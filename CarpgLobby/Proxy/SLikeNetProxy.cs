using CarpgLobby.Extensions;
using CarpgLobby.Properties;
using CarpgLobby.Provider;
using CarpgLobby.Utils;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CarpgLobby.Proxy
{
    public class SLikeNetProxy
    {
        public delegate int Callback(IntPtr ptr);

        [DllImport("SLikeNetProxy.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool InitProxy(int players, int port, [MarshalAs(UnmanagedType.FunctionPtr)]Callback callback);

        [DllImport("SLikeNetProxy.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void SetVersion([MarshalAs(UnmanagedType.LPStr)]string version);

        [DllImport("SLikeNetProxy.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void ShutdownProxy();

        public void Init()
        {
            if (!InitProxy(Settings.Default.ProxyPlayers, Settings.Default.ProxyPort, HandleMessage))
                throw new Exception("Failed to init proxy.");
        }

        public int HandleMessage(IntPtr ptr)
        {
            int len = Marshal.ReadInt32(ptr);
            byte[] data = new byte[len];
            ptr = IntPtr.Add(ptr, 4);
            Marshal.Copy(ptr, data, 0, len);

            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                try
                {
                    return ProcessMessage(reader);
                }
                catch(ProviderException)
                {
                    return -1;
                }
                catch(Exception ex)
                {
                    Logger.Error($"Unhandled exception: {ex}");
                    return -1;
                }
            }
        }

        private int ProcessMessage(BinaryReader reader)
        {
            MsgType type = (MsgType)reader.ReadByte();
            switch (type)
            {
                case MsgType.MSG_INFO:
                case MsgType.MSG_ERROR:
                    {
                        string msg = reader.ReadStringSimple();
                        if (type == MsgType.MSG_INFO)
                            Logger.Info(msg);
                        else
                            Logger.Error(msg);
                        return 0;
                    }
                case MsgType.MSG_CREATE_SERVER:
                    {
                        string name = reader.ReadStringSimple();
                        string guid = reader.ReadStringSimple();
                        string ip = reader.ReadStringSimple();
                        int players = reader.ReadInt32();
                        int flags = reader.ReadInt32();
                        Server server = Lobby.Instance.CreateServer(new Server
                        {
                            Name = name,
                            Guid = guid,
                            MaxPlayers = players,
                            Flags = flags
                        }, ip);
                        return server.ServerID;
                    }
                case MsgType.MSG_UPDATE_SERVER:
                    {
                        string ip = reader.ReadStringSimple();
                        int id = reader.ReadInt32();
                        int players = reader.ReadInt32();
                        Lobby.Instance.UpdateServer(id, players, ip);
                        return 0;
                    }
                case MsgType.MSG_REMOVE_SERVER:
                    {
                        string ip = reader.ReadStringSimple();
                        int id = reader.ReadInt32();
                        bool lost_connection = reader.ReadBoolean();
                        Lobby.Instance.DeleteServer(id, ip, lost_connection);
                        return 0;
                    }
                default:
                    return 0;
            }
        }

        public void Shutdown()
        {
            ShutdownProxy();
        }
    }
}
