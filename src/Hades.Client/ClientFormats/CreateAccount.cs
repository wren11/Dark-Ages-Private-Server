#region

using Darkages.Network;

#endregion

namespace DAClient.ClientFormats
{
    public class CreateAccount : NetworkFormat
    {
        public string AislingPassword;
        public string AislingUsername;

        public CreateAccount(string aislingUsername, string aislingPassword)
        {
            AislingUsername = aislingUsername;
            AislingPassword = aislingPassword;
        }

        public override bool Secured => true;
        public override byte Command => 0x02;

        public override void Serialize(NetworkPacketReader reader)
        {
            AislingUsername = reader.ReadStringA();
            AislingPassword = reader.ReadStringA();
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.WriteStringA(AislingUsername);
            writer.WriteStringA(AislingPassword);
        }
    }
}