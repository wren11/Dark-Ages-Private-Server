namespace Darkages.Network.ClientFormats
{
    public class ClientFormat7B : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x7B;

        public byte Type { get; set; }

        #region Type 0 Variables

        public string Name { get; set; }

        #endregion

        public override void Serialize(NetworkPacketReader reader)
        {
            Type = reader.ReadByte();

            #region Type 0

            if (Type == 0x00)
                Name = reader.ReadStringA();

            #endregion

            #region Type 1

            if (Type == 0x01)
            {
            }

            #endregion
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }

        #region Type 1 Variables

        #endregion
    }
}