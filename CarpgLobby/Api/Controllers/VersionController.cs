using CarpgLobby.Api.Model;
using CarpgLobby.Provider;
using CarpgLobby.Utils;
using System.Web.Http;

namespace CarpgLobby.Api.Controllers
{
    public class VersionController : BaseController
    {
        // Get version
        [Route("api/version")]
        public GetVersionResponse GetVersion()
        {
            return HandleRequest(() =>
            {
                Logger.Verbose($"Get version from {Ip}.");
                return new GetVersionResponse
                {
                    Ok = true,
                    Version = Utils.Version.Current
                };
            });
        }

        // Set version
        [Route("api/version")]
        public BaseResponse PostVersion([FromUri]string version, [FromUri]string key)
        {
            return HandleRequest(() =>
            {
                Lobby.Instance.SetVersion(version, key, Ip);
                return new BaseResponse { Ok = true };
            });
        }
    }
}
