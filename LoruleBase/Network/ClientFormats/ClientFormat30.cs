#region

using Darkages.Types;

#endregion

namespace Darkages.Network.ClientFormats
{
    public class ClientFormat30 : NetworkFormat
    {
        public byte MovingFrom;
        public byte MovingTo;
        public Pane PaneType;

        public ClientFormat30()
        {
            Secured = true;
            Command = 0x30;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
            PaneType = (Pane) reader.ReadByte();
            MovingFrom = reader.ReadByte();
            MovingTo = reader.ReadByte();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}