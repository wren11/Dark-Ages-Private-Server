namespace Darkages.Network.ServerFormats
{
    public class ServerFormat3A : NetworkFormat
    {
        public ServerFormat3A()
        {
            Icon = (ushort)IconStatus.Available;
        }

        public ServerFormat3A(ushort icon, byte length)
        {
            Icon = icon;
            Length = length;
        }

        public ServerFormat3A(ushort icon, byte length, byte status)
        {
            //            Icon = IconStatus.status + icon;
        }

        public override bool Secured => true;
        public override byte Command => 0x3A;

        public ushort Icon { get; set; }
        public byte Length { get; set; }

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