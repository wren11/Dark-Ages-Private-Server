namespace Darkages.Network.ClientFormats
{
    public class ClientFormat26 : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x26;

        public string Username { get; set; }
        public string Password { get; set; }
        public string NewPassword { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Username = reader.ReadStringA();
            Password = reader.ReadStringA();
            NewPassword = reader.ReadStringA();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}