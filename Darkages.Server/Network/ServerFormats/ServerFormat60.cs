namespace Darkages.Network.ServerFormats
{
    public class ServerFormat60 : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x60;

        public byte Type { get; set; }


        public uint Hash { get; set; }


        public byte[] Data { get; set; }
        public ushort Size { get; set; }


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