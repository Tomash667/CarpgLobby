using CarpgLobby.Api.Model;
using CarpgLobby.Provider;
using CarpgLobby.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace CarpgLobby.Api.Controllers
{
    public class UpdateController : BaseController
    {
        [Route("api/update/{from}")]
        public GetUpdateResponse GetUpdate(int from)
        {
            return HandleRequest(() =>
            {
                Logger.Verbose($"Get update from {Ip}.");
                List<UpdateDto> updates = Lobby.Instance.GetUpdates(from);
                if (updates != null)
                {
                    return new GetUpdateResponse
                    {
                        Ok = true,
                        Files = updates.OrderBy(x => x.To)
                            .Select(x => new Update { Version = x.To, Path = x.Path })
                            .ToList()
                    };
                }
                else
                {
                    return new GetUpdateResponse
                    {
                        Ok = false,
                        Error = "Can't find update path for this version."
                    };
                }
            });
        }

        [Route("api/update")]
        [TokenAuthentication]
        public BaseResponse PostUpdate([FromBody] PostUpdateRequest request)
        {
            return HandleRequest(() =>
            {
                Logger.Info($"Post update from {request.From} to {request.To}.");
                UpdateDto update = new UpdateDto
                {
                    From = Utils.Version.ParseVersion(request.From),
                    To = Utils.Version.ParseVersion(request.To),
                    Path = request.Path
                };
                Lobby.Instance.AddUpdate(update);
                return new BaseResponse { Ok = true };
            });
        }
    }
}
