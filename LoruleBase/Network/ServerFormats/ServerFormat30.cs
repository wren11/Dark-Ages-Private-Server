#region

using Darkages.Network.Game;
using Darkages.Types;

#endregion

namespace Darkages.Network.ServerFormats
{
    public class ReactorInputSequence : ServerFormat30
    {
        private readonly string _captionA;
        private readonly int _inputLength;
        private readonly Mundane _mundane;

        public ReactorInputSequence(Mundane mundane, string captionA, int inputLength = 48)
        {
            _mundane = mundane;
            _captionA = captionA;
            _inputLength = inputLength;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write((byte)0x04);
            writer.Write((byte)0x01);
            writer.Write((uint)_mundane.Serial);
            writer.Write((byte)0x00);
            writer.Write(_mundane.Template.Image);
            writer.Write((byte)0x05);
            writer.Write((byte)0x05);
            writer.Write(ushort.MinValue);
            writer.Write((byte)0x00);
            writer.Write(ushort.MinValue);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.WriteStringA("test");
            writer.WriteStringB("test");
            writer.WriteStringA(_captionA);
            writer.Write((byte)_inputLength);
        }
    }

    public class ReactorSequence : ServerFormat30
    {
        private readonly GameClient client;
        private readonly DialogSequence sequence;

        public ReactorSequence(GameClient gameClient, DialogSequence sequenceMenu)
            : base(gameClient)
        {
            client = gameClient;
            sequence = sequenceMenu;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            if (!client.Aisling.LoggedIn)
                return;

            writer.Write((byte)0x00);
            writer.Write((byte)0x01);
            writer.Write((uint)sequence.Id);
            writer.Write((byte)0x00);
            writer.Write(sequence.DisplayImage);
            writer.Write((byte)0x00);
            writer.Write((byte)0x01);
            writer.Write(ushort.MinValue);
            writer.Write((byte)0x00);
            writer.Write(ushort.MaxValue);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write(sequence.CanMoveBack);
            writer.Write(sequence.CanMoveNext);
            writer.Write((byte)0);
            writer.WriteStringA(sequence.Title);
            writer.WriteStringB(sequence.DisplayText);
        }
    }

    public class ServerFormat30 : NetworkFormat
    {
        private readonly GameClient _client;

        public ServerFormat30()
        {
            Secured = true;
            Command = 0x30;
        }

        public ServerFormat30(GameClient gameClient) : this()
        {
            _client = gameClient;
        }

        public ServerFormat30(GameClient gameClient, Dialog sequenceMenu)
            : this(gameClient)
        {
            Sequence = sequenceMenu;
        }

        public Dialog Sequence { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write((byte)0x00);
            writer.Write((byte)0x01);
            writer.Write((uint)_client.DlgSession.Serial);
            writer.Write((byte)0x00);
            writer.Write(Sequence.DisplayImage);
            writer.Write((byte)0x00);
            writer.Write((byte)0x01);
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