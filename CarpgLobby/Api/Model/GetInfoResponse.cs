using System;
using System.Collections.Generic;

namespace CarpgLobby.Api.Model
{
    public class GetInfoResponse : BaseResponse
    {
        public TimeSpan Uptime { get; set; }
        public string Version { get; set; }
        public Dictionary<string, int> Stats { get; set; }
    }
}
