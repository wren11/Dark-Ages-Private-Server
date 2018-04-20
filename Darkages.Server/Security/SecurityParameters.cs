using Darkages.Common;
using Darkages.Network;
using System;
using System.Text;
using IFormattable = Darkages.Network.IFormattable;

namespace Darkages.Security
{
    [Serializable]
    public sealed class SecurityParameters : IFormattable
    {
        public static readonly SecurityParameters Default
            = new SecurityParameters(0, Encoding.ASCII.GetBytes(ServerContext.Config?.DefaultKey ?? "NexonInc."));

        public SecurityParameters()
        {
            Seed = (byte)Generator.Random.Next(0, 9);
            Salt = Generator.GenerateString(9).ToByteArray();
        }

        public SecurityParameters(byte seed, byte[] key)
        {
            Seed = seed;
            Salt = key;
        }

        public byte Seed { get; private set; }
        public byte[] Salt { get; private set; }

        public void Serialize(NetworkPacketReader reader)
        {
            Seed = reader.ReadByte();
            Salt = reader.ReadBytes(reader.ReadByte());
        }

        public void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Seed);
            writer.Write(
                (byte)Salt.Length);
            writer.Write(Salt);
        }
    }
}