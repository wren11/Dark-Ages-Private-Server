#region

using Darkages.Network;

#endregion

namespace DAClient.ClientFormats
{
    public class RedirectRequest : NetworkFormat
    {
        private readonly string key;
        private readonly string name;
        private readonly byte seed;
        private readonly uint socketid;

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
            writer.Write(seed);
            writer.WriteStringA(key);
            writer.WriteStringA(name);
            writer.Write(socketid);
            writer.Write(0x00);
        }
    }
}