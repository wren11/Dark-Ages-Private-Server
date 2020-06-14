namespace Darkages.Network.ClientFormats
{
    public class ClientFormat7B : NetworkFormat
    {
        public ClientFormat7B()
        {
            Secured = true;
            Command = 0x7B;
        }

        public string Name { get; set; }
        public byte Type { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Type = reader.ReadByte();

            #region Type 0

            if (Type == 0x00)
                Name = reader.ReadStringA();

            #endregion Type 0

            #region Type 1

            if (Type == 0x01 && reader.Packet.Data.Length > 2) Name = reader.ReadStringB();

            #endregion Type 1
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}