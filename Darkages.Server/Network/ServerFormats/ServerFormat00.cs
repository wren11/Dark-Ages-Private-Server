using Darkages.Security;

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat00 : NetworkFormat
    {
        public override bool Secured => false;

        public override byte Command => 0x00;

        public SecurityParameters Parameters { get; set; }
        public byte Type { get; set; }
        public uint Hash { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Type);

            if (Type == 0)
            {
                writer.Write(Hash);
                writer.Write(Parameters);
            }
        }
    }
}