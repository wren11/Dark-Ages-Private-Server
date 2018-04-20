namespace Darkages.Network.ServerFormats
{
    public class ServerFormat0D : NetworkFormat
    {
        public enum MsgType : byte
        {
            Chant = 2,
            Normal = 0,
            Shout = 1
        }

        public override bool Secured => true;

        public override byte Command => 0x0D;

        public byte Type { get; set; }
        public int Serial { get; set; }
        public string Text { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Type);
            writer.Write(Serial);
            writer.WriteStringA(Text);
        }
    }
}