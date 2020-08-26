using System;

namespace CarpgLobby.Provider
{
    class ServerDto
    {
        public int ServerID { get; set; }
        public string Guid { get; set; }
        public DateTime LastUpdate { get; set; }
        public string Name { get; set; }
        public int Flags { get; set; }
        public int Players { get; set; }
        public int MaxPlayers { get; set; }
        public int Version { get; set; }
    }
}
