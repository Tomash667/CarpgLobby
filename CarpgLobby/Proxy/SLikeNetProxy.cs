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
        private delegate int Callback(IntPtr ptr);
        private static Callback callback;

        [DllImport("SLikeNetProxy.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern bool InitProxy(int players, int port, [MarshalAs(UnmanagedType.FunctionPtr)]Callback callback);

        [DllImport("SLikeNetProxy.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern void ShutdownProxy();

        [DllImport("SLikeNetProxy.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern void RestartProxy();

        public void Init()
        {
            callback = new Callback(HandleMessage);
            if (!InitProxy(Settings.Default.ProxyPlayers, Settings.Default.ProxyPort, callback))
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
                catch (ProviderException)
                {
                    return -1;
                }
                catch (Exception ex)
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
                case MsgType.MSG_VERBOSE:
                case MsgType.MSG_INFO:
                case MsgType.MSG_WARNING:
                case MsgType.MSG_ERROR:
                    {
                        string msg = reader.ReadStringSimple();
                        switch (type)
                        {
                            case MsgType.MSG_VERBOSE:
                                Logger.Verbose(msg);
                                break;
                            case MsgType.MSG_INFO:
                                Logger.Info(msg);
                                break;
                            case MsgType.MSG_WARNING:
                                Logger.Warning(msg);
                                break;
                            case MsgType.MSG_ERROR:
                                Logger.Error(msg);
                                break;
                        }
                        return 0;
                    }
                case MsgType.MSG_CREATE_SERVER:
                    {
                        string name = reader.ReadStringSimple();
                        string guid = reader.ReadStringSimple();
                        string ip = reader.ReadStringSimple();
                        int players = reader.ReadInt32();
                        int flags = reader.ReadInt32();
                        int version = reader.ReadInt32();
                        Server server = Lobby.Instance.CreateServer(new Server
                        {
                            Name = name,
                            Guid = guid,
                            MaxPlayers = players,
                            Flags = flags,
                            Version = version
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
                case MsgType.MSG_STAT:
                    {
                        StatType stat = (StatType)reader.ReadByte();
                        Lobby.Instance.UpdateStat(stat);
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

        public void Restart()
        {
            RestartProxy();
        }
    }
}
