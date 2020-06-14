#region

using Darkages.Types;
using System;
using System.Net;
using System.Text;

#endregion

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat03 : NetworkFormat
    {
        public ServerFormat03()
        {
            Secured = false;
            Command = 0x03;
        }

        public IPEndPoint EndPoint { get; set; }

        public Redirect Redirect { get; set; }
        public byte Remaining => (byte)(Redirect.Salt.Length + Redirect.Name.Length + 7);

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(EndPoint);
            writer.Write(Remaining);
            writer.Write(Convert.ToByte(Redirect.Seed));
            writer.Write(
                (byte)Redirect.Salt.Length);
            writer.Write(Encoding.UTF8.GetBytes(Redirect.Salt));
            writer.WriteStringA(Redirect.Name);
            writer.Write(Convert.ToInt32(Redirect.Serial));
        }
    }
}