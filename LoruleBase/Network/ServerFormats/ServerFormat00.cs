#region

using Darkages.Security;

#endregion

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat00 : NetworkFormat
    {
        public ServerFormat00()
        {
            Secured = false;
            Command = 0x00;
        }

        public uint Hash { get; set; }
        public SecurityParameters Parameters { get; set; }
        public byte Type { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Type);

            {
                writer.Write(Hash);
                writer.Write(Parameters);
            }
        }
    }
}