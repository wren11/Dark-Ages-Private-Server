#region

using System;
using System.Globalization;

#endregion

namespace Darkages.Network
{
    public static class NetworkFormatManager
    {
        static NetworkFormatManager()
        {
            ClientFormats = new Type[256];

            for (var i = 0; i < 256; i++)
            {
                ClientFormats[i] = Type.GetType(string.Format(CultureInfo.CurrentCulture, "Darkages.Network.ClientFormats.ClientFormat{0:X2}", i), false, false);
            }
        }

        public static Type[] ClientFormats { get; }

        public static NetworkFormat GetClientFormat(byte command)
        {
            return Activator.CreateInstance(ClientFormats[command]) as NetworkFormat;
        }
    }
}