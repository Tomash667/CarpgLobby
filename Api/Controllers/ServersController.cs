using CarpgLobby.Api.Model;
using CarpgLobby.Provider;
using CarpgLobby.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;

namespace CarpgLobby.Api.Controllers
{
    public class ServersController : ApiController
    {
        // Get list of servers
        public GetServersResponse Get()
        {
            return HandleRequest(() =>
            {
                List<Model.Server> servers = Lobby.Instance.GetServers(out int timestamp, Ip);
                return new GetServersResponse
                {
                    Ok = true,
                    Servers = servers,
                    Timestamp = timestamp
                };
            });
        }

        // Get list of changes
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

        // Register server
        public CreateServerResponse Post([FromBody]CreateServerRequest request)
        {
            return HandleRequest(() =>
            {
                var server = Lobby.Instance.CreateServer(new Provider.Server
                {
                    Name = request.Name,
                    MaxPlayers = request.Players,
                    Flags = request.Flags,
                    Ip = $"{Ip}:{request.Port}"
                }, Ip);
                return new CreateServerResponse
                {
                    Ok = true,
                    ServerID = server.ServerID,
                    Key = server.Key
                };
            });
        }

        // Update server
        public BaseResponse Put(int id, [FromUri]string key, [FromUri]int players)
        {
            return HandleRequest(() =>
            {
                Lobby.Instance.UpdateServer(new Provider.Server
                {
                    ServerID = id,
                    Key = key,
                    Players = players
                }, Ip);
                return new BaseResponse { Ok = true };
            });
        }

        // Notify that server is still runing
        [ActionName("ping")]
        public BaseResponse PutPing(int id, [FromUri]string key)
        {
            return HandleRequest(() =>
            {
                Lobby.Instance.PingServer(id, key, Ip);
                return new BaseResponse { Ok = true };
            });
        }

        // Remove server
        public BaseResponse Delete(int id, [FromUri]string key)
        {
            return HandleRequest(() =>
            {
                Lobby.Instance.DeleteServer(id, key, Ip);
                return new BaseResponse { Ok = true };
            });
        }

        private T HandleRequest<T>(Func<T> func) where T : BaseResponse, new()
        {
            try
            {
                return func();
            }
            catch(ProviderException ex)
            {
                return new T
                {
                    Ok = false,
                    Error = ex.Message
                };
            }
            catch(Exception ex)
            {
                Logger.Error($"Unhandled exception from {Ip}: {ex}");
                return new T
                {
                    Ok = false,
                    Error = ex.Message
                };
            }
        }

        private string Ip => Request.GetOwinContext()?.Request?.RemoteIpAddress ?? "unknown";
    }
}
