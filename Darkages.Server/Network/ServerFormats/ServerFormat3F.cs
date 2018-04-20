namespace Darkages.Network.ServerFormats
{
    public class ServerFormat3F : NetworkFormat
    {
        public ServerFormat3F(byte pane, byte slot, int time)
        {
            Pane = pane;
            Slot = slot;
            Time = time;
        }

        public override bool Secured => true;
        public override byte Command => 0x3F;

        public byte Pane { get; set; }
        public byte Slot { get; set; }
        public int Time { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write((byte)Pane);
            writer.Write((byte)Slot);
            writer.Write((int)Time);
        }
    }
}