using System.IO;

namespace CarpgLobby.Extensions
{
    public static class BinaryReaderExtensions
    {
        public static string ReadStringSimple(this BinaryReader reader)
        {
            int len = reader.ReadInt16();
            return new string(reader.ReadChars(len));
        }
    }
}
