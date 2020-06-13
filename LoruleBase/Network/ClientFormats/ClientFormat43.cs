namespace Darkages.Network.ClientFormats
{
    public class ClientFormat43 : NetworkFormat
    {
        public int Serial;
        public byte Type;

        public ClientFormat43()
        {
            Secured = true;
            Command = 0x43;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
            Type = reader.ReadByte();

            if (Type == 0x01)
                Serial = reader.ReadInt32();

            if (Type == 0x02)
            {
            }


            if (Type == 0x03)
            {
                X = reader.ReadInt16();
                Y = reader.ReadInt16();
            }
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }

        #region Type 3 Variables

        public short X { get; set; }

        public short Y { get; set; }

        #endregion
    }
}