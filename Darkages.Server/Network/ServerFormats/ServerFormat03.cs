using Darkages.Types;
using System.Net;

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat03 : NetworkFormat
    {
        public override bool Secured => false;

        public override byte Command => 0x03;

        public IPEndPoint EndPoint { get; set; }

        public byte Remaining => (byte)(Redirect.Salt.Length + Redirect.Name.Length + 7);

        public Redirect Redirect { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(EndPoint);
            writer.Write(Remaining);
            writer.Write(
                Redirect.Seed);
            writer.Write(
                (byte)Redirect.Salt.Length);
            writer.Write(Redirect.Salt);
            writer.WriteStringA(Redirect.Name);
            writer.Write(Redirect.Serial);
        }
    }
}