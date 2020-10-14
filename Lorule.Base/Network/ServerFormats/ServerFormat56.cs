namespace Darkages.Network.ServerFormats
{
    public class ServerFormat56 : NetworkFormat
    {
        public byte[] Data;

        public ushort Size;

        public ServerFormat56()
        {
            Secured = true;
            Command = 0x56;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Size);
            writer.Write(Data);
            writer.Write((byte) 0x02);
        }
    }
}