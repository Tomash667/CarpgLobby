using System.Collections.Generic;

namespace CarpgLobby.Api.Model
{
    public class GetServersResponse : BaseResponse
    {
        public IEnumerable<Server> Servers { get; set; }
        public int Timestamp { get; set; }
        public string Version { get; set; }
    }
}
