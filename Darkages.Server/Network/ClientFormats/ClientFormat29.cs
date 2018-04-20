using System;

namespace Darkages.Network.ClientFormats
{
    public class ClientFormat29 : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x29;


        public uint ID { get; set; }
        public byte ItemSlot { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            ItemSlot = reader.ReadByte();
            ID = reader.ReadUInt32();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {

        }
    }
}
