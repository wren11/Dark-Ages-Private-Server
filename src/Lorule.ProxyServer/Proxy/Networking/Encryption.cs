
using System.Text;

namespace Proxy.Networking
{
    public sealed class CryptoParams
    {
        public byte[] PrivateKey { get; set; }

        public byte SeedByte { get; set; }

        public CryptoParams()
        {
            PrivateKey = Encoding.ASCII.GetBytes("NexonInc.");
            SeedByte = 0;
        }

        public CryptoParams(byte seedByte, byte[] privateKey)
        {
            PrivateKey = privateKey;
            SeedByte = seedByte;
        }
    }
}
