using System;

namespace CarpgLobby.Api.Model
{
    public class GetInfoResponse : BaseResponse
    {
        public TimeSpan Uptime { get; set; }
        public int Servers { get; set; }
        public int TotalServers { get; set; }
        public int Changes { get; set; }
        public int Timestamp { get; set; }
        public int Errors { get; set; }
    }
}
