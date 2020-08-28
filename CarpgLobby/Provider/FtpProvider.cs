using CarpgLobby.Properties;
using FluentFTP;
using System;
using System.IO;

namespace CarpgLobby.Provider
{
    class FtpProvider
    {
        private static readonly string path = "/domains/carpg.pl/public_html/carpgdata/wersja.bin";
        private static readonly uint sign = 0x475052CA;

        public bool Enabled => !string.IsNullOrWhiteSpace(Settings.Default.FtpHost);

        public int GetVersion()
        {
            if (!Enabled)
                return 0;

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
            if (!Enabled)
                return;

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
                    client.Upload(stream, path, FtpRemoteExists.Overwrite);
                }
            }
        }

        private FtpClient GetClient()
        {
            FtpClient client = new FtpClient
            {
                Host = Settings.Default.FtpHost,
                Credentials = new System.Net.NetworkCredential(Settings.Default.FtpLogin, Settings.Default.FtpPassword)
            };
            return client;
        }
    }
}
