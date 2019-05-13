using System.Collections.Generic;

namespace CarpgLobby.Api.Model
{
    public class GetChangelogResponse : BaseResponse
    {
        public Dictionary<string, string> Changes { get; set; }
    }
}
