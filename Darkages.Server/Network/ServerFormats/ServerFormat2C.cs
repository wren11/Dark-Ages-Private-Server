namespace Darkages.Network.ServerFormats
{
    public class ServerFormat2C : NetworkFormat
    {
        public ServerFormat2C(byte slot, short icon, string text)
        {
            Slot = slot;
            Icon = icon;
            Text = text;
        }

        public override bool Secured => true;

        public override byte Command => 0x2C;

        public byte Slot { get; set; }
        public short Icon { get; set; }
        public string Text { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Slot);
            writer.Write(Icon);
            writer.WriteStringA(Text);
        }
    }
}