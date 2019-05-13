﻿using CarpgLobby.Api.Model;
using CarpgLobby.Proxy;
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

        public Server CreateServer(Server server, string ip)
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
            changes.Add(new Change
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
            Server server = GetServer(id, ip, "update");
            if (players < 1 || players > server.MaxPlayers)
            {
                Logger.Error($"Validation failed for update server {server.ServerID}: players [{players}/{server.MaxPlayers}], from {ip}.");
                throw new ProviderException("Can't update server, validation failed.");
            }
            server.Players = players;
            server.LastUpdate = DateTime.Now;
            changes.Add(new Change
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
            Server server = GetServer(id, ip, "delete");
            servers.Remove(server);
            changes.Add(new Change
            {
                Type = ChangeType.Remove,
                ServerID = id,
                Timestamp = timestamp++,
                Date = DateTime.Now
            });
            Logger.Info($"Delete server {id} from {ip}{(lost_connection ? " (lost connection)" : "")}.");
        }

        public List<Api.Model.Server> GetServers(out int timestamp)
        {
            timestamp = changes.Count;
            return servers.Select(x => Map(x)).ToList();
        }

        public List<Api.Model.Change> GetChanges(ref int timestamp)
        {
            int t = timestamp;
            timestamp = this.timestamp;
            var groupped = changes.Where(x => x.Timestamp >= t)
                .GroupBy(x => x.ServerID);
            List<Api.Model.Change> result = new List<Api.Model.Change>();
            foreach (var group in groupped)
            {
                bool added = false, updated = false, removed = false;
                foreach (Change change in group)
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
                        result.Add(new Api.Model.Change
                        {
                            Type = ChangeType.Remove,
                            ServerID = group.Key
                        });
                    }
                }
                else if (added || updated)
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

        private Server GetServer(int id, string ip, string action)
        {
            Server server = servers.SingleOrDefault(x => x.ServerID == id);
            if (server == null)
            {
                Logger.Error($"Missing server {id} to {action} from {ip}.");
                throw new ProviderException("Missing server.");
            }
            return server;
        }

        private Api.Model.Server Map(Server server)
        {
            return new Api.Model.Server
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
    }
}
