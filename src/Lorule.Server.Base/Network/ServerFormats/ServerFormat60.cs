namespace Darkages.Network.ServerFormats
{
    public class ServerFormat60 : NetworkFormat
    {
        public byte[] Data;
        public uint Hash;
        public ushort Size;

        public byte Type;

        public ServerFormat60()
        {
            Secured = true;
            Command = 0x60;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Type);

            if (Type == 0x00)
                writer.Write(Hash);

            if (Type == 0x01)
            {
                writer.Write(Size);
                writer.Write(Data);
            }
        }
    }
}