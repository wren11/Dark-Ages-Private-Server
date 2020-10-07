#region

using System;

#endregion

namespace Darkages.Network.ClientFormats
{
    public class ClientFormat45 : NetworkFormat
    {
        public ClientFormat45()
        {
            Secured = true;
            Command = 0x45;
        }

        public DateTime Ping { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Ping = DateTime.UtcNow;
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}