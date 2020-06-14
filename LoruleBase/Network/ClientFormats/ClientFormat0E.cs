namespace Darkages.Network.ClientFormats
{
    public class ClientFormat0E : NetworkFormat
    {
        public ClientFormat0E()
        {
            Secured = true;
            Command = 0x0E;
        }

        public enum MsgType : byte
        {
            Normal = 0,
            Shout = 1,
            Chant = 2
        }

        public string Text { get; set; }
        public byte Type { get; set; }

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