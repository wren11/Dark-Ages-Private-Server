namespace Darkages.Network.ServerFormats
{
    public class ServerFormat10 : NetworkFormat
    {
        public ServerFormat10(byte slot) : this()
        {
            Slot = slot;
        }

        public ServerFormat10()
        {
            Secured = true;
            Command = 0x10;
        }

        public byte Slot { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Slot);
        }
    }
}