namespace Darkages.Network.ServerFormats
{
    public class ServerFormat3C : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x3C;

        public ushort Line { get; set; }
        public byte[] Data { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Line);
            writer.Write(Data);
        }
    }
}