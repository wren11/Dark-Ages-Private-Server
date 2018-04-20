namespace Darkages.Network.ServerFormats
{
    public class ServerFormat66 : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x66;

        public byte Type => 0x03;

        public string Text =>
            "https://classicrpgcharacter.nexon.com/service/ConfirmGameUser.aspx?id=%s&pw=%s&mainCode=2&subCode=0";

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Type);
            writer.WriteStringA(Text);
        }
    }
}