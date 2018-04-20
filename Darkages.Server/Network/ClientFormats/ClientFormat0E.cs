namespace Darkages.Network.ClientFormats
{
    public class ClientFormat0E : NetworkFormat
    {
        public enum MsgType : byte
        {
            Normal = 0,
            Shout = 1,
            Chant = 2
        }

        public override bool Secured => true;
        public override byte Command => 0x0E;

        public byte Type { get; set; }
        public string Text { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Type = reader.ReadByte();
            Text = reader.ReadStringA();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}