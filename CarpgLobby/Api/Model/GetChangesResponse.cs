using System.Collections.Generic;

namespace CarpgLobby.Api.Model
{
    public class GetChangesResponse : BaseResponse
    {
        public int Timestamp { get; set; }
        public IEnumerable<Change> Changes { get; set; }
    }
}
