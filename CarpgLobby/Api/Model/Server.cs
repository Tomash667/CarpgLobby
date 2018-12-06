namespace CarpgLobby.Api.Model
{
    public class Server
    {
        public int ID { get; set; }
        public string Guid { get; set; }
        public string Name { get; set; }
        public int Players { get; set; }
        public int MaxPlayers { get; set; }
        public int Flags { get; set; }
    }
}
