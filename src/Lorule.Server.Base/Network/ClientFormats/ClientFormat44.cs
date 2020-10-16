namespace Darkages.Network.ClientFormats
{
    public class ClientFormat44 : NetworkFormat
    {
        public byte Slot;

        public ClientFormat44()
        {
            Secured = true;
            Command = 0x44;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
            Slot = reader.ReadByte();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}