namespace CarpgLobby.Api.Model
{
    public class Update
    {
        public int Version { get; set; }
        public string Path { get; set; }
        public uint Crc { get; set; }
    }
}
