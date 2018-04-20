namespace Darkages.Network.ServerFormats
{
    public class ServerFormat7E : NetworkFormat
    {
        public override bool Secured => false;

        public override byte Command => 0x7E;

        public byte Type => 0x1B;

        public string Text => ServerContext.Config?.HandShakeMessage ?? "CUNTS\n";


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