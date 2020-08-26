using System.Collections.Generic;

namespace CarpgLobby.Api.Model
{
    public class GetUpdateResponse : BaseResponse
    {
        public List<Update> Files { get; set; }
    }
}
