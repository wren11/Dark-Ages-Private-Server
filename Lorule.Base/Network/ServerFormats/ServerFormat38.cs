namespace Darkages.Network.ServerFormats
{
    public class ServerFormat38 : NetworkFormat
    {
        public ServerFormat38(byte slot) : this()
        {
            Slot = slot;
        }

        public ServerFormat38()
        {
            Secured = true;
            Command = 0x38;
        }

        public byte Slot { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter packet)
        {
            packet.Write(Slot);
        }
    }
}