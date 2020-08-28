namespace CarpgLobby.Api.Model
{
    public class PostUpdateRequest
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Path { get; set; }
        public uint Crc { get; set; }
    }
}
