using System;

namespace CarpgLobby.Utils
{
    public static class Version
    {
        public static int ParseVersion(string str)
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

        public static string ToString(int version)
        {
            int major = ((version & 0xFF0000) >> 16);
            int minor = ((version & 0xFF00) >> 8);
            int patch = (version & 0xFF);
            if (patch == 0)
                return $"{major}.{minor}";
            else
                return $"{major}.{minor}.{patch}";
        }
    }
}
