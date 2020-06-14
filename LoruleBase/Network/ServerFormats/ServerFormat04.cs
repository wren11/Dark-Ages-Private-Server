namespace Darkages.Network.ServerFormats
{
    public class ServerFormat04 : NetworkFormat
    {
        public ServerFormat04(Aisling aisling) : this()
        {
            X = (short)aisling.X;
            Y = (short)aisling.Y;
        }

        public ServerFormat04()
        {
            Secured = true;
            Command = 0x04;
        }

        public short X { get; set; }
        public short Y { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(X);
            writer.Write(Y);
            writer.Write(
                (short)0x000B);
            writer.Write(
                (short)0x000B);
        }
    }
}