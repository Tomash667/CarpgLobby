namespace CarpgLobby.Api.Model
{
    public class CreateServerResponse : BaseResponse
    {
        public int ServerID { get; set; }
        public string Key { get; set; }
    }
}
