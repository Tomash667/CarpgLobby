using CarpgLobby.Api.Model;
using CarpgLobby.Provider;
using System.Collections.Generic;
using System.Web.Http;

namespace CarpgLobby.Api.Controllers
{
    public class ServersController : BaseController
    {
        // Get info
        [Route("api/servers/info")]
        public GetInfoResponse GetInfo()
        {
            return HandleRequest(() =>
            {
                return Lobby.Instance.GetInfo(Ip);
            });
        }

        // Get list of servers
        [Route("api/servers")]
        public GetServersResponse Get()
        {
            return HandleRequest(() =>
            {
                List<Model.Server> servers = Lobby.Instance.GetServers(out int timestamp, Ip);
                return new GetServersResponse
                {
                    Ok = true,
                    Servers = servers,
                    Timestamp = timestamp,
                    Version = Utils.Version.Current
                };
            });
        }

        // Get list of changes
        [Route("api/servers/{id}")]
        public GetChangesResponse GetChanges(int id)
        {
            return HandleRequest(() =>
            {
                int timestamp = id;
                List<Model.Change> changes = Lobby.Instance.GetChanges(ref timestamp, Ip);
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
