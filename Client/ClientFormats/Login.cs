using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Darkages.Network;

namespace DAClient.ClientFormats
{
    class Login : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x03;

        protected string _username, _password;

        public Login(string username, string password)
        {
            _username = username;
            _password = password;
        }

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
