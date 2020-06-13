namespace Darkages.Network.ClientFormats
{
    public class ClientFormat03 : NetworkFormat
    {
        public ClientFormat03()
        {
            Secured = true;
            Command = 0x03;
        }

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