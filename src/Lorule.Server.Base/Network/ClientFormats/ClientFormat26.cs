namespace Darkages.Network.ClientFormats
{
    public class ClientFormat26 : NetworkFormat
    {
        public ClientFormat26()
        {
            Secured = true;
            Command = 0x26;
        }

        public string NewPassword { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }

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