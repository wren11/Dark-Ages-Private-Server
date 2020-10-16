#region

using Darkages.Network;

#endregion

namespace DAClient.ClientFormats
{
    internal class Login : NetworkFormat
    {
        protected string _username, _password;

        public Login(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public override bool Secured => true;

        public override byte Command => 0x03;

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.WriteStringA(_username);
            writer.WriteStringA(_password);
        }
    }
}