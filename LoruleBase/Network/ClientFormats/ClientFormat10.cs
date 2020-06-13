#region

using Darkages.Security;

#endregion

namespace Darkages.Network.ClientFormats
{
    public class ClientFormat10 : NetworkFormat
    {
        public ClientFormat10()
        {
            Secured = false;
            Command = 0x10;
        }

        public SecurityParameters Parameters { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Parameters = reader.ReadObject<SecurityParameters>();
            Name = reader.ReadStringA();
            Id = reader.ReadInt32();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}