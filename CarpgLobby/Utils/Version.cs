using System;
using System.IO;

namespace CarpgLobby.Utils
{
    public static class Version
    {
        private static string ver;

        private static string Path => AppDomain.CurrentDomain.BaseDirectory + "version.txt";

        public static string Current
        {
            get
            {
                if (ver == null)
                {
                    try
                    {
                        Current = File.ReadAllText(Path);
                        Number = ParseVersion(Current);
                    }
                    catch (Exception ex)
                    {
                        if (!(ex is FileNotFoundException))
                            Logger.Error(ex.ToString());
                        Current = "0";
                        Number = 0;
                    }
                }
                return ver;
            }
            set
            {
                Number = ParseVersion(value);
                ver = value;
                File.WriteAllText(Path, value);
            }
        }

        public static int Number { get; private set; }

        private static int ParseVersion(string str)
        {
            string[] parts = str.Split(new char[] { '.' });
            if (parts.Length < 1 || parts.Length > 3)
                throw new ArgumentException("Invalid version string.");
            int major = int.Parse(parts[0]);
            int minor = 0,
                patch = 0;
            if (parts.Length > 1)
            {
                minor = int.Parse(parts[1]);
                if (parts.Length > 2)
                    patch = int.Parse(parts[2]);
            }
            return ((major & 0xFF) << 16) | ((minor & 0xFF) << 8) | (patch & 0xFF);
        }
    }
}
