namespace Darkages.Network.ClientFormats
{
    public class ClientFormat3B : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x3B;

        public override void Serialize(NetworkPacketReader reader)
        {
            if (reader.CanRead)
            {
                Type = reader.ReadByte();

                if (reader.CanRead)
                {
                    BoardIndex = reader.ReadUInt16();
                }

                if (reader.CanRead)
                {
                    TopicIndex = reader.ReadUInt16();
                }

                if (Type == 0x06)
                {
                    reader.Position = 0;
                    reader.ReadByte();
                    BoardIndex = reader.ReadUInt16();

                    To = reader.ReadStringA();
                    Title = reader.ReadStringA();
                    Message = reader.ReadStringB();
                }
                else if (Type == 0x04)
                {
                    reader.Position = 0;
                    reader.ReadByte();
                    BoardIndex = reader.ReadUInt16();

                    Title = reader.ReadStringA();
                    Message = reader.ReadStringB();
                }
            }
        }

        public ushort TopicIndex { get; set; }
        public ushort BoardIndex { get; set; }
        public byte Type { get; set; }
        public string To, Title, Message;

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}