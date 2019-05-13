using CarpgLobby.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace CarpgLobby.Provider
{
    class Repository
    {
        public Dictionary<string, string> changelog;
        public int version;
        private static string Path => AppDomain.CurrentDomain.BaseDirectory + "db.json";

        public Repository()
        {
            Reset();
        }

        public void Save()
        {
            try
            {
                using (StreamWriter file = File.CreateText(Path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, this);
                }
                Logger.Info("Saved repository.");
            }
            catch(Exception ex)
            {
                Logger.Error($"Failed to save repository: {ex}");
            }
        }

        public void Load()
        {
            if (!File.Exists(Path))
                return;

            try
            {
                using (StreamReader file = File.OpenText(Path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Populate(file, this);
                }
                Logger.Info("Loaded repository.");
            }
            catch(Exception ex)
            {
                Logger.Error($"Failed to load repository: {ex}");
                Reset();
            }
        }

        private void Reset()
        {
            changelog = new Dictionary<string, string>();
            version = 0;
        }
    }
}
