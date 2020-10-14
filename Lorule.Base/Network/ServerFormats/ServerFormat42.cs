#region

using System;
using Darkages.Types;

#endregion

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat42 : NetworkFormat
    {
        public Item ExchangedItem;

        public string Message;

        public Aisling Player;

        public byte Stage;

        public byte Type;

        public ServerFormat42()
        {
            Secured = true;
            Command = 0x42;
        }

        public ServerFormat42(Aisling user, byte type = 0x00, byte method = 0x00, string lpMsg = "", Item lpItem = null)
        {
            if (method <= 0)
                throw new ArgumentOutOfRangeException(nameof(method));

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