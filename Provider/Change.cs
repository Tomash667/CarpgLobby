using CarpgLobby.Api.Model;
using System;

namespace CarpgLobby.Provider
{
    public class Change
    {
        public ChangeType Type { get; set; }
        public int ServerID { get; set; }
        public int Timestamp { get; set; }
        public DateTime Date { get; set; }
    }
}
