namespace Darkages.Network.ServerFormats
{
    public class ServerFormat2C : NetworkFormat
    {
        public short Icon;


        public byte Slot;
        public string Text;

        public ServerFormat2C()
        {
            Command = 0x2C;
            Secured = true;
        }

        public ServerFormat2C(byte slot, short icon, string text) : this()
        {
            Slot = slot;
            Icon = icon;
            Text = text;
        }

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