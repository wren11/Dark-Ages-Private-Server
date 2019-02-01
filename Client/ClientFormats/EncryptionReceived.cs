using Darkages.Network;

namespace DAClient.ClientFormats
{
    public class EncryptionReceived : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x57;

        private byte _type;

        public EncryptionReceived(byte type)
        {
            _type = type;
        }

        public override void Serialize(NetworkPacketReader reader)
        {

        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            if (_type == 0)
            {
                writer.Write((byte)0x00);
                writer.Write((byte)0x00);
                writer.Write((short)0x00);
            }
            else
            {
                writer.Write((byte)0x01);
                writer.Write((short)0x00);
                writer.Write((short)0x00);
            }
        }
    }
}
