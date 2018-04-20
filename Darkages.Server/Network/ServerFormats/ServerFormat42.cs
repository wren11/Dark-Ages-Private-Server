using Darkages.Types;

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat42 : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x42;

        public Aisling Player { get; set; }

        public Item ExchangedItem { get; set; }

        public byte Stage { get; set; }

        public byte Type { get; set; }

        public string Message { get; set; }

        public ServerFormat42(Aisling user, byte type = 0x00, byte method = 0x00, string lpMsg = "", Item lpItem = null)
        {
            Stage = type;
            Player = user;
            ExchangedItem = lpItem;
            Message = lpMsg;
        }

        public override void Serialize(NetworkPacketReader reader)
        {

        }

        public override void Serialize(NetworkPacketWriter writer)
        {

        }
    }
}
