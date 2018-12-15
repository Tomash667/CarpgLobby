using CarpgLobby.Proxy;
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
                    }
                    catch (Exception)
                    {
                        Current = "0";
                    }
                }
                return ver;
            }
            set
            {
                ver = value;
                File.WriteAllText(Path, value);
                SLikeNetProxy.SetVersion(value);
            }
        }
    }
}
