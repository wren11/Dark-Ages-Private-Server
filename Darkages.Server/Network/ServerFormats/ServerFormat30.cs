using Darkages.Network.Game;
using Darkages.Types;

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat30 : NetworkFormat
    {
        private readonly GameClient _client;

        public ServerFormat30(GameClient gameClient, Dialog sequenceMenu)
        {
            _client = gameClient;
            Sequence = sequenceMenu;
        }

        public override bool Secured => true;
        public override byte Command => 0x30;

        public Dialog Sequence { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write((byte)0x00); // type!
            writer.Write((byte)0x01); // ??
            writer.Write((uint)_client.DlgSession.Serial);
            writer.Write((byte)0x00); // ??
            writer.Write(Sequence.DisplayImage);
            writer.Write((byte)0x00); // ??
            writer.Write((byte)0x01); // ??
            writer.Write(ushort.MinValue);
            writer.Write((byte)0x00);
            writer.Write(ushort.MinValue);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write(Sequence.CanMoveBack);
            writer.Write(Sequence.CanMoveNext);
            writer.Write((byte)0);
            writer.WriteStringA(Sequence.Current.Title);
            writer.WriteStringB(Sequence.Current.DisplayText);
        }
    }
}