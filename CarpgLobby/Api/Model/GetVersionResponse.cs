namespace CarpgLobby.Api.Model
{
    public class GetVersionResponse : BaseResponse
    {
        public int Version { get; set; }
        public string VersionString { get; set; }
    }
}
