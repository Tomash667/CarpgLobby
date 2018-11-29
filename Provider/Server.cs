using System;

namespace CarpgLobby.Provider
{
    public class Server
    {
        public int ServerID { get; set; }
        public string Key { get; set; }
        public DateTime LastUpdate { get; set; }
        public string Name { get; set; }
        public int Flags { get; set; }
        public int Players { get; set; }
        public int MaxPlayers { get; set; }
    }
}
