#region

using System;

#endregion

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat3B : NetworkFormat
    {
        public DateTime Ping;

        public ServerFormat3B()
        {
            Secured = true;
            Command = 0x3B;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            Ping = DateTime.UtcNow;

            writer.Write((ushort)0x0001);
        }
    }
}