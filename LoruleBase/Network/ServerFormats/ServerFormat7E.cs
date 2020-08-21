namespace Darkages.Network.ServerFormats
{
    public class ServerFormat7E : NetworkFormat
    {
        public string Text = ServerContext.Config?.HandShakeMessage ?? "CUNTS\n";
        public byte Type = 0x1B;

        public ServerFormat7E()
        {
            Secured = false;
            Command = 0x7E;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Type);
            writer.WriteString(Text);
        }
    }
}