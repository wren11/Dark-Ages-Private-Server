namespace Darkages.Network.ClientFormats
{
    public class ClientFormat43 : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x43;

        public byte Type { get; private set; }

        #region Type 1 Variables

        /// <summary>
        ///     The serial number of the sprite that the client clicked. (Type 1 Variable)
        /// </summary>
        public int Serial { get; private set; }

        #endregion

        public override void Serialize(NetworkPacketReader reader)
        {
            Type = reader.ReadByte();

            if (Type == 0x01)
                Serial = reader.ReadInt32();

            if (Type == 0x02)
            {
                // ???
            }

            if (Type == 0x03)
            {
                // 43 4A 03 00 1A 00 2D 00 00
                X = reader.ReadInt16();
                Y = reader.ReadInt16();
            }
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }

        #region Type 3 Variables

        /// <summary>
        ///     The X component of the coordinate of the tile that the client clicked. (Type 3 Variable)
        /// </summary>
        public short X { get; set; }

        /// <summary>
        ///     The Y component of the coordinate of the tile that the client clicked. (Type 3 Variable)
        /// </summary>
        public short Y { get; set; }

        #endregion
    }
}