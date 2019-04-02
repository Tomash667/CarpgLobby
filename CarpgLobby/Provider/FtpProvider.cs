using CarpgLobby.Properties;
using FluentFTP;
using System;
using System.IO;

namespace CarpgLobby.Provider
{
    public class FtpProvider
    {
        private static readonly string path = "/domains/carpg.pl/public_html/carpgdata/wersja";
        private static readonly uint sign = 0x475052CA;

        public int GetVersion()
        {
            using (var client = GetClient())
            using (var stream = new BinaryReader(client.OpenRead(path)))
            {
                uint read_sign = stream.ReadUInt32();
                if (read_sign != sign)
                    throw new Exception("Invalid version signature!");
                return stream.ReadInt32();
            }
        }

        public void SetVersion(int version)
        {
            using (var client = GetClient())
            {
                if (client.FileExists(path))
                    client.DeleteFile(path);
                using (MemoryStream stream = new MemoryStream())
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(sign);
                    writer.Write(version);
                    client.UploadDataType = FtpDataType.Binary;
                    client.Upload(stream, path, FtpExists.Overwrite);
                }
            }
        }

        private FtpClient GetClient()
        {
            FtpClient client = new FtpClient();
            client.Host = Settings.Default.FtpHost;
            client.Credentials = new System.Net.NetworkCredential(Settings.Default.FtpLogin, Settings.Default.FtpPassword);
            return client;
        }
    }
}
