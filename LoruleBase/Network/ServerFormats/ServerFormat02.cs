namespace Darkages.Network.ServerFormats
{
    public class ServerFormat02 : NetworkFormat
    {
        public ServerFormat02(byte code, string text)
        {
            Secured = true;
            Command = 0x02;
            Code = code;
            Text = text;
        }

        public byte Code { get; set; }
        public string Text { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Code);
            writer.WriteStringA(Text);
        }
    }
}