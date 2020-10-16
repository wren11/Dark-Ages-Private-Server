namespace Darkages.Network.ClientFormats
{
    public class ClientFormat57 : NetworkFormat
    {
        public byte Slot;

        public byte Type;

        public ClientFormat57()
        {
            Secured = true;
            Command = 0x57;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
            Type = reader.ReadByte();
            if (reader.GetCanRead())
                Slot = reader.ReadByte();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}