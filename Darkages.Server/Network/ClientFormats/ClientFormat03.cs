namespace Darkages.Network.ClientFormats
{
    public class ClientFormat03 : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x03;

        public string Username { get; set; }
        public string Password { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Username = reader.ReadStringA();
            Password = reader.ReadStringA();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}