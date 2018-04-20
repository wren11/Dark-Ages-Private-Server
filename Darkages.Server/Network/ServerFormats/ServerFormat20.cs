namespace Darkages.Network.ServerFormats
{
    public class ServerFormat20 : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x20;

        public byte Shade { get; set; }

        public byte Unknown => 0x01;

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Shade);
            writer.Write(Unknown);
        }
    }
}