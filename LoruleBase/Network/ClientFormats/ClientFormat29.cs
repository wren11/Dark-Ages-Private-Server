namespace Darkages.Network.ClientFormats
{
    public class ClientFormat29 : NetworkFormat
    {
        public uint ID;
        public byte ItemSlot;

        public ClientFormat29()
        {
            Secured = true;
            Command = 0x29;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
            ItemSlot = reader.ReadByte();
            ID = reader.ReadUInt32();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}