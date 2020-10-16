#region

using Darkages.Network;

#endregion

namespace DAClient.ClientFormats
{
    public class SendMessageBox : NetworkFormat
    {
        public SendMessageBox(byte code, string text)
        {
            Code = code;
            Text = text;
        }

        public byte Code { get; set; }
        public string Text { get; set; }

        public override bool Secured => true;
        public override byte Command => 0x02;

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Code);
            writer.WriteStringA(Text);
        }
    }
    public class Assail : NetworkFormat
    {
        public override byte Command => 0x13;
        public override bool Secured => true;

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write((byte) 0x01);
        }
    }
}