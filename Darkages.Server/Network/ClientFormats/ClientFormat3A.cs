namespace Darkages.Network.ClientFormats
{
    public class ClientFormat3A : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x3A;

        public ushort ScriptId { get; set; }
        public ushort Step { get; set; }
        public uint Serial { get; set; }


        public override void Serialize(NetworkPacketReader reader)
        {
            var type = reader.ReadByte();
            var id = reader.ReadUInt32();
            var scriptid = reader.ReadUInt16();
            var step = reader.ReadUInt16();


            ScriptId = scriptid;
            Step = step;
            Serial = id;
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}