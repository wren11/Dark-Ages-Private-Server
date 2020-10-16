#region

using Darkages.Types;

#endregion

namespace Darkages.Network.ClientFormats
{
    public class ClientFormat79 : NetworkFormat
    {
        public ClientFormat79()
        {
            Secured = true;
            Command = 0x79;
        }

        public ActivityStatus Status { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Status = (ActivityStatus) reader.ReadByte();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}