using CarpgLobby.Api.Model;
using CarpgLobby.Properties;
using CarpgLobby.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarpgLobby.Provider
{
    public class Lobby
    {
        public static Lobby Instance;
        private readonly List<Server> servers = new List<Server>();
        private readonly List<Change> changes = new List<Change>();
        private int nextId = 1;
        private int timestamp = 0;

        public Server CreateServer(Server server, string ip)
        {
            server.Name = server.Name.Trim();
            if (server.Name.Length == 0 || server.MaxPlayers <= 1)
            {
                Logger.Error($"Validation failed for create server: name [{server.Name}], players [{server.MaxPlayers}], from {ip}.");
                throw new ProviderException("Can't create server, validation failed.");
            }
            server.ServerID = nextId;
            nextId += 1;
            server.Players = 1;
            server.LastUpdate = DateTime.Now;
            server.Key = KeyGen.GetKey();
            servers.Add(server);
            changes.Add(new Change
            {
                Type = ChangeType.Add,
                ServerID = server.ServerID,
                Timestamp = timestamp++,
                Date = DateTime.Now
            });
            Logger.Info($"Created server {server.ServerID} '{server.Name}', players {server.MaxPlayers}, flags {server.Flags} at {server.Ip}.");
            return server;
        }

        public void UpdateServer(Server serverInfo, string ip)
        {
            Server server = GetServer(serverInfo.ServerID, serverInfo.Key, ip, "update");
            if (serverInfo.Players > server.MaxPlayers)
            {
                Logger.Error($"Validation failed for update server {server.ServerID}: players [{serverInfo.Players}/{server.MaxPlayers}], from {ip}.");
                throw new ProviderException("Can't update server, validation failed.");
            }
            server.Players = serverInfo.Players;
            server.LastUpdate = DateTime.Now;
            changes.Add(new Change
            {
                Type = ChangeType.Update,
                ServerID = serverInfo.ServerID,
                Timestamp = timestamp++,
                Date = DateTime.Now
            });
            Logger.Info($"Updated server {server.ServerID} players {server.Players}/{server.MaxPlayers} by {ip}.");
        }

        public void PingServer(int id, string key, string ip)
        {
            Server server = GetServer(id, key, ip, "ping");
            server.LastUpdate = DateTime.Now;
            Logger.Info($"Ping server {id} from {ip}.");
        }

        public void DeleteServer(int id, string key, string ip)
        {
            Server server = GetServer(id, key, ip, "delete");
            servers.Remove(server);
            changes.Add(new Change
            {
                Type = ChangeType.Remove,
                ServerID = id,
                Timestamp = timestamp++,
                Date = DateTime.Now
            });
            Logger.Info($"Delete server {id} from {ip}.");
        }

        public List<Api.Model.Server> GetServers(out int timestamp, string ip)
        {
            Logger.Verbose($"Get servers from {ip}.");
            timestamp = changes.Count;
            return servers.Select(x => Map(x)).ToList();
        }

        public List<Api.Model.Change> GetChanges(ref int timestamp, string ip)
        {
            Logger.Verbose($"Get server changes at {timestamp} from {ip}.");
            int t = timestamp;
            timestamp = this.timestamp;
            var groupped = changes.Where(x => x.Timestamp >= t)
                .GroupBy(x => x.ServerID);
            List<Api.Model.Change> result = new List<Api.Model.Change>();
            foreach(var group in groupped)
            {
                bool added = false, updated = false, removed = false;
                foreach(Change change in group)
                {
                    if (change.Type == ChangeType.Add)
                        added = true;
                    else if (change.Type == ChangeType.Update)
                        updated = true;
                    else
                        removed = true;
                }
                if(removed)
                {
                    if(!added)
                    {
                        result.Add(new Api.Model.Change
                        {
                            Type = ChangeType.Remove,
                            ServerID = group.Key
                        });
                    }
                }
                else if(added || updated)
                {
                    Server server = servers.Single(x => x.ServerID == group.Key);
                    result.Add(new Api.Model.Change
                    {
                        Type = updated ? ChangeType.Update : ChangeType.Add,
                        Server = Map(server)
                    });
                }
            }
            return result;
        }

        public void RefreshServers()
        {
            Logger.Verbose("Refreshing servers.");
            DateTime now = DateTime.Now;
            List<Server> old = servers.Where(x => (now - x.LastUpdate) > Settings.Default.ServerRefreshTime).ToList();
            foreach(Server server in old)
            {
                Logger.Info($"Removed old server {server.ServerID} '{server.Name}.");
                changes.Add(new Change
                {
                    Type = ChangeType.Remove,
                    ServerID = server.ServerID,
                    Timestamp = timestamp++,
                    Date = DateTime.Now
                });
                servers.Remove(server);
            }
            changes.RemoveAll(x => (now - x.Date) > Settings.Default.ChangeTrackingTime);
        }

        private Server GetServer(int id, string key, string ip, string action)
        {
            Server server = servers.SingleOrDefault(x => x.ServerID == id);
            if (server == null || server.Key != key)
            {
                if (server == null)
                    Logger.Error($"Missing server {id} to {action} from {ip}.");
                else if (server.Key != key)
                    Logger.Error($"Invalid server {id} key '{key}' to {action} from {ip}.");
                throw new ProviderException("Missing server.");
            }
            return server;
        }

        private Api.Model.Server Map(Server server)
        {
            return new Api.Model.Server
            {
                ID = server.ServerID,
                Ip = server.Ip,
                Name = server.Name,
                Players = server.Players,
                MaxPlayers = server.MaxPlayers,
                Flags = server.Flags
            };
        }
    }
}
