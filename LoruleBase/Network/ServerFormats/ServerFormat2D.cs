namespace Darkages.Network.ServerFormats
{
    public class ServerFormat2D : NetworkFormat
    {
        public byte Slot;

        public ServerFormat2D(byte slot) : this()
        {
            Slot = slot;
        }

        public ServerFormat2D()
        {
            Command = 0x2D;
            Secured = true;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Slot);
        }
    }
}