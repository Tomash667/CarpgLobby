using System;

namespace CarpgLobby.Provider
{
    class ProviderException : Exception
    {
        public ProviderException(string msg) : base(msg)
        {
        }
    }
}
