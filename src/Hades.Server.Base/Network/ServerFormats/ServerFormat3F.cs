namespace Darkages.Network.ServerFormats
{
    public class ServerFormat3F : NetworkFormat
    {
        public byte Pane;
        public byte Slot;
        public int Time;

        public ServerFormat3F()
        {
            Secured = true;
            Command = 0x3F;
        }

        public ServerFormat3F(byte pane, byte slot, int time) : this()
        {
            Pane = pane;
            Slot = slot;
            Time = time;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Pane);
            writer.Write(Slot);
            writer.Write(Time);
        }
    }
}