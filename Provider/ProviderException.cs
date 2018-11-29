using System;

namespace CarpgLobby.Provider
{
    public class ProviderException : Exception
    {
        public ProviderException(string msg) : base(msg)
        {
        }
    }
}
