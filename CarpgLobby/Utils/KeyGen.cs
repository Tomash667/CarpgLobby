using System;
using System.Text;

namespace CarpgLobby.Utils
{
    public static class KeyGen
    {
        public static string GetKey()
        {
            Guid guid = Guid.NewGuid();
            string str = Convert.ToBase64String(guid.ToByteArray());
            StringBuilder sb = new StringBuilder();
            foreach(char c in str)
            {
                if (char.IsLetterOrDigit(c))
                    sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
