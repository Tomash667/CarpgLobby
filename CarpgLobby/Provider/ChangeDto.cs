using CarpgLobby.Api.Model;
using System;

namespace CarpgLobby.Provider
{
    class ChangeDto
    {
        public ChangeType Type { get; set; }
        public int ServerID { get; set; }
        public int Timestamp { get; set; }
        public DateTime Date { get; set; }
    }
}
