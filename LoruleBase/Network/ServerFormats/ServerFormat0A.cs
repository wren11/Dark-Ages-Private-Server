namespace Darkages.Network.ServerFormats
{
    public class ServerFormat0A : NetworkFormat
    {
        public ServerFormat0A(byte type, string text) : this()
        {
            Type = type;
            Text = text;
        }

        public ServerFormat0A()
        {
            Secured = true;
            Command = 0x0A;
        }

        public enum MsgType
        {
            Action = 2,
            Board = 10,
            Dialog = 9,
            Global = 3,
            Guild = 12,
            Message = 1,
            Party = 11,
            Whisper = 0,
            Test = 8
        }

        public string Text { get; set; }
        public byte Type { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Type);

            if (!string.IsNullOrEmpty(Text))
                writer.WriteStringB(Text);
        }
    }
}