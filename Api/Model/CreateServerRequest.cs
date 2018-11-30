namespace CarpgLobby.Api.Model
{
    public class CreateServerRequest
    {
        public string Name { get; set; }
        public int Players { get; set; }
        public int Flags { get; set; }
        public int Port { get; set; }
    }
}
