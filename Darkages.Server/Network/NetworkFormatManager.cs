using System;

namespace Darkages.Network
{
    public static class NetworkFormatManager
    {
        static NetworkFormatManager()
        {
            ClientFormats = new Type[256];
            ServerFormats = new Type[256];

            for (var i = 0; i < 256; i++)
            {
                ClientFormats[i] = Type.GetType(
                    string.Format("Darkages.Network.ClientFormats.ClientFormat{0:X2}", i), false, false);
                ServerFormats[i] = Type.GetType(
                    string.Format("Darkages.Network.ServerFormats.ServerFormat{0:X2}", i), false, false);
            }
        }

        public static Type[] ClientFormats { get; }
        public static Type[] ServerFormats { get; }

        public static NetworkFormat GetClientFormat(byte command)
        {
            return Activator.CreateInstance(ClientFormats[command]) as NetworkFormat;
        }

        public static NetworkFormat GetServerFormat(byte command)
        {
            return Activator.CreateInstance(ServerFormats[command]) as NetworkFormat;
        }

        public static bool TryGetClientFormat(byte command, out NetworkFormat format)
        {
            return (format = GetClientFormat(command)) != null;
        }

        public static bool TryGetServerFormat(byte command, out NetworkFormat format)
        {
            return (format = GetServerFormat(command)) != null;
        }
    }
}