namespace Darkages.Network.ClientFormats
{
    public class ClientFormat3A : NetworkFormat
    {
        public ClientFormat3A()
        {
            Secured = true;
            Command = 0x3A;
        }

        public string Input { get; set; }
        public ushort ScriptId { get; set; }
        public uint Serial { get; set; }
        public ushort Step { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            var type = reader.ReadByte();
            var id = reader.ReadUInt32();
            var scriptid = reader.ReadUInt16();
            var step = reader.ReadUInt16();

            if (reader.ReadByte() == 0x02)
                if (reader.GetCanRead())
                    Input = reader.ReadStringA();

            ScriptId = scriptid;
            Step = step;
            Serial = id;
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}