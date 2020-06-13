namespace Darkages.Network.ClientFormats
{
    public class ClientFormat3B : NetworkFormat
    {
        public string To, Title, Message;

        public ClientFormat3B()
        {
            Secured = true;
            Command = 0x3B;
        }

        public ushort TopicIndex { get; set; }
        public ushort BoardIndex { get; set; }
        public byte Type { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            if (reader.GetCanRead())
            {
                Type = reader.ReadByte();

                if (reader.GetCanRead()) BoardIndex = reader.ReadUInt16();

                if (reader.GetCanRead()) TopicIndex = reader.ReadUInt16();

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

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}