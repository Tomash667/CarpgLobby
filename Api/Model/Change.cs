namespace CarpgLobby.Api.Model
{
    public class Change
    {
        public ChangeType Type { get; set; }
        public Server Server { get; set; }
        public int ServerID { get; set; }
    }
}
