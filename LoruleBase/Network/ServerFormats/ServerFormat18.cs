namespace Darkages.Network.ServerFormats
{
    public class ServerFormat18 : NetworkFormat
    {
        public byte Slot;

        public ServerFormat18(byte slot) : this()
        {
            Slot = slot;
        }

        public ServerFormat18()
        {
            Secured = true;
            Command = 0x18;
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