using CarpgLobby.Api.Model;
using CarpgLobby.Proxy;
using CarpgLobby.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarpgLobby.Provider
{
    class Lobby
    {
        public static Lobby Instance;
        private readonly List<ServerDto> servers = new List<ServerDto>();
        private readonly List<ChangeDto> changes = new List<ChangeDto>();
        private int nextId = 1;
        private int timestamp = 0;
        private int totalServers = 0, connections = 0, totalConnections = 0;
        public int requests = 0;
        private readonly DateTime startDate = DateTime.Now;
        private readonly Repository repository = new Repository();
        public string VersionStr
        {
            get { return Utils.Version.ToString(repository.version); }
            set { repository.version = Utils.Version.ParseVersion(value); }
        }
        public int Version => repository.version;

        public Lobby()
        {
            repository.Load();
        }

        public GetInfoResponse GetInfo()
        {
            return new GetInfoResponse
            {
                Ok = true,
                Uptime = (DateTime.Now - startDate),
                Version = VersionStr,
                Stats = new Dictionary<string, int>
                {
                    { "CurrentServers", servers.Count },
                    { "TotalServers", totalServers },
                    { "Timestamp", timestamp },
                    { "Changes", changes.Count },
                    { "Errors", Logger.Errors },
                    { "Connections", connections },
                    { "TotalConnections", totalConnections },
                    { "Requests", requests }
                }
            };
        }

        public ServerDto CreateServer(ServerDto server, string ip)
        {
            server.Name = server.Name.Trim();
            if (server.Name.Length == 0 || server.MaxPlayers < 1)
            {
                Logger.Error($"Validation failed for create server: name [{server.Name}], players [{server.MaxPlayers}], from {ip}.");
                throw new ProviderException("Can't create server, validation failed.");
            }
            server.ServerID = nextId;
            nextId += 1;
            server.Players = 1;
            server.LastUpdate = DateTime.Now;
            servers.Add(server);
            ++totalServers;
            changes.Add(new ChangeDto
            {
                Type = ChangeType.Add,
                ServerID = server.ServerID,
                Timestamp = timestamp++,
                Date = DateTime.Now
            });
            Logger.Info($"Created server {server.ServerID} '{server.Name}', players {server.MaxPlayers}, flags {server.Flags} at {ip}.");
            return server;
        }

        public void UpdateServer(int id, int players, string ip)
        {
            ServerDto server = GetServer(id, ip, "update");
            if (players < 1 || players > server.MaxPlayers)
            {
                Logger.Error($"Validation failed for update server {server.ServerID}: players [{players}/{server.MaxPlayers}], from {ip}.");
                throw new ProviderException("Can't update server, validation failed.");
            }
            server.Players = players;
            server.LastUpdate = DateTime.Now;
            changes.Add(new ChangeDto
            {
                Type = ChangeType.Update,
                ServerID = id,
                Timestamp = timestamp++,
                Date = DateTime.Now
            });
            Logger.Info($"Updated server {server.ServerID} players {server.Players}/{server.MaxPlayers} by {ip}.");
        }

        public void DeleteServer(int id, string ip, bool lost_connection)
        {
            ServerDto server = GetServer(id, ip, "delete");
            servers.Remove(server);
            changes.Add(new ChangeDto
            {
                Type = ChangeType.Remove,
                ServerID = id,
                Timestamp = timestamp++,
                Date = DateTime.Now
            });
            Logger.Info($"Delete server {id} from {ip}{(lost_connection ? " (lost connection)" : "")}.");
        }

        public List<Server> GetServers(out int timestamp)
        {
            timestamp = changes.Count;
            return servers.Select(x => Map(x)).ToList();
        }

        public List<Change> GetChanges(ref int timestamp)
        {
            int t = timestamp;
            timestamp = this.timestamp;
            var groupped = changes.Where(x => x.Timestamp >= t)
                .GroupBy(x => x.ServerID);
            List<Change> result = new List<Change>();
            foreach (var group in groupped)
            {
                bool added = false, updated = false, removed = false;
                foreach (ChangeDto change in group)
                {
                    if (change.Type == ChangeType.Add)
                        added = true;
                    else if (change.Type == ChangeType.Update)
                        updated = true;
                    else
                        removed = true;
                }
                if (removed)
                {
                    if (!added)
                    {
                        result.Add(new Change
                        {
                            Type = ChangeType.Remove,
                            ServerID = group.Key
                        });
                    }
                }
                else if (added || updated)
                {
                    ServerDto server = servers.Single(x => x.ServerID == group.Key);
                    result.Add(new Change
                    {
                        Type = updated ? ChangeType.Update : ChangeType.Add,
                        Server = Map(server)
                    });
                }
            }
            return result;
        }

        private ServerDto GetServer(int id, string ip, string action)
        {
            ServerDto server = servers.SingleOrDefault(x => x.ServerID == id);
            if (server == null)
            {
                Logger.Error($"Missing server {id} to {action} from {ip}.");
                throw new ProviderException("Missing server.");
            }
            return server;
        }

        private Server Map(ServerDto server)
        {
            return new Server
            {
                ID = server.ServerID,
                Guid = server.Guid,
                Name = server.Name,
                Players = server.Players,
                MaxPlayers = server.MaxPlayers,
                Flags = server.Flags,
                Version = server.Version
            };
        }

        public void SetVersion(string version)
        {
            VersionStr = version;
            repository.Save();
            FtpProvider ftp = new FtpProvider();
            ftp.SetVersion(Utils.Version.ParseVersion(version));
        }

        public void UpdateStat(StatType stat)
        {
            if (stat == StatType.STAT_CONNECT)
            {
                ++connections;
                ++totalConnections;
            }
            else if (stat == StatType.STAT_DISCONNECT)
                --connections;
        }

        public Dictionary<string, string> GetChangelog(string lang)
        {
            if (string.IsNullOrWhiteSpace(lang))
                return repository.changelog;
            else
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                if (repository.changelog.TryGetValue(lang, out string changelog))
                    result[lang] = changelog;
                else if (repository.changelog.TryGetValue("en", out changelog))
                    result["en"] = changelog;
                return result;
            }
        }

        public string GetChangelogSimple(string lang)
        {
            if (string.IsNullOrWhiteSpace(lang))
                lang = "en";
            if (!repository.changelog.TryGetValue(lang, out string changelog) && lang != "en")
                repository.changelog.TryGetValue("en", out changelog);
            return changelog;
        }

        public void SetChangelog(string lang, string changelog)
        {
            if (string.IsNullOrWhiteSpace(lang))
                throw new Exception("Invalid language.");
            if (string.IsNullOrWhiteSpace(changelog))
                repository.changelog.Remove(lang);
            else
                repository.changelog[lang] = changelog;
            repository.Save();
        }

        public void AddUpdate(UpdateDto update)
        {
            if (Utils.Version.IsPatch(update.From))
                throw new ProviderException("Update patch must be 0.");
            if (repository.updates.Any(x => x.To == update.To))
                throw new ProviderException("Update already added.");
            repository.updates.Add(update);
            repository.Save();
        }

        public bool CanUpdate(int from)
        {
            return GetUpdates(from) != null;
        }

        public List<UpdateDto> GetUpdates(int from)
        {
            List<UpdateDto> updates = new List<UpdateDto>();
            if (from == Version)
                return updates;
            from = Utils.Version.RemovePatch(from);
            int current = Version;
            int len = 0;
            while (true)
            {
                UpdateDto update = repository.updates.SingleOrDefault(x => x.To == current);
                if (update == null)
                    return null;
                updates.Add(update);
                if (Utils.Version.IsPatch(current))
                    current = Utils.Version.RemovePatch(current);
                else
                    current = Utils.Version.Previous(current);
                if (current == from)
                    return updates;
                ++len;
                if (len > 5)
                    return null;
            }
        }
    }
}
