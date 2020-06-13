#region

using Darkages.Types;

#endregion

namespace Darkages.Network.ClientFormats
{
    public class ClientFormat07 : NetworkFormat
    {
        public ClientFormat07()
        {
            Secured = true;
            Command = 0x07;
        }


        public Position Position { get; set; }
        public byte PickupType { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            PickupType = reader.ReadByte();
            Position = reader.ReadPosition();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}