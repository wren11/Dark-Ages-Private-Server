using Darkages.Network;

namespace DAClient.ClientFormats
{
    public class RedirectRequest : NetworkFormat
    {
        private byte seed;
        private string key;
        private string name;
        private uint socketid;

        public RedirectRequest(byte seed, string key, string name, uint socketid)
        {
            this.seed = seed;
            this.key = key;
            this.name = name;
            this.socketid = socketid;
        }

        public override bool Secured => false;

        public override byte Command => 0x10;


        public Client _client { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {

        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write((byte)seed);
            writer.WriteStringA(key);
            writer.WriteStringA(name);
            writer.Write((uint)socketid);
            writer.Write(0x00);
        }
    }
}
