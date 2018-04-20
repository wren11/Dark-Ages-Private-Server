using Darkages.Types;
using System;

namespace Darkages.Network.ClientFormats
{
    public class ClientFormat0F : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x0F;

        public byte Index { get; set; }
        public Position Point { get; set; }
        public uint Serial { get; set; }
        public string Data { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            string data = CHeckData(reader);

            reader.Position = 0;
            Index = reader.ReadByte();

            try
            {
                if (reader.CanRead)
                    Serial = reader.ReadUInt32();

                if (reader.Position + 4 < reader.Packet.Data.Length)
                    Point = reader.ReadPosition();
            }
            catch (Exception)
            {
                //ignore
            }
            finally
            {
                Data = data.Trim('\0');
            }
        }

        private string CHeckData(NetworkPacketReader reader)
        {
            Index = reader.ReadByte();

            var @data = string.Empty;
            var @char = (default(char));

            do
            {
                @char = Convert.ToChar(reader.ReadByte());
                data += new string(@char, 1);
            }
            while (@char != Char.Parse("\0"));
            return data;
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}