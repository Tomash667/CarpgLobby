using CarpgLobby.Api.Model;
using CarpgLobby.Provider;
using CarpgLobby.Utils;
using System.Collections.Generic;
using System.Web.Http;

namespace CarpgLobby.Api.Controllers
{
    public class ServersController : BaseController
    {
        // Get info
        [Route("api/servers/info")]
        [TokenAuthentication]
        public GetInfoResponse GetInfo()
        {
            return HandleRequest(() =>
            {
                Logger.Verbose($"Get info from {Ip}.");
                return Lobby.Instance.GetInfo();
            });
        }

        // Get list of servers
        [Route("api/servers")]
        public GetServersResponse Get()
        {
            return HandleRequest(() =>
            {
                Logger.Verbose($"Get servers from {Ip}.");
                List<Model.Server> servers = Lobby.Instance.GetServers(out int timestamp);
                return new GetServersResponse
                {
                    Ok = true,
                    Servers = servers,
                    Timestamp = timestamp,
                    Version = Lobby.Instance.VersionStr
                };
            });
        }

        // Get list of changes
        [Route("api/servers/{id}")]
        public GetChangesResponse GetChanges(int id)
        {
            return HandleRequest(() =>
            {
                Logger.Verbose($"Get server changes at {id} from {Ip}.");
                int timestamp = id;
                List<Model.Change> changes = Lobby.Instance.GetChanges(ref timestamp);
                return new GetChangesResponse
                {
                    Ok = true,
                    Changes = changes,
                    Timestamp = timestamp
                };
            });
        }
    }
}
