namespace Darkages.Network.ServerFormats
{
    public class ServerFormat3A : NetworkFormat
    {
        public ushort Icon;
        public byte Length;

        public ServerFormat3A()
        {
            Secured = true;
            Command = 0x3A;
        }

        public ServerFormat3A(ushort icon, byte length) : this()
        {
            Icon = icon;
            Length = length;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Icon);
            writer.Write(Length);
        }

        private enum IconStatus : ushort
        {
            Active = 0,
            Available = 266,
            Unavailable = 532
        }
    }
}