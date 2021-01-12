#region

using System;
using Darkages.Types;

#endregion

namespace Darkages.Network.ClientFormats
{
    public class ClientFormat0F : NetworkFormat
    {
        public ClientFormat0F()
        {
            Secured = true;
            Command = 0x0F;
        }

        public string Data { get; set; }
        public byte Index { get; set; }
        public Position Point { get; set; }
        public uint Serial { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            var data = CHeckData(reader);

            reader.Position = 0;
            Index = reader.ReadByte();

            try
            {
                if (reader.GetCanRead())
                    Serial = reader.ReadUInt32();

                if (reader.Position + 4 < reader.Packet.Data.Length)
                    Point = reader.ReadPosition();
            }
            catch (Exception ex)
            {
                ServerContext.Logger(ex.Message, Microsoft.Extensions.Logging.LogLevel.Error);
                ServerContext.Logger(ex.StackTrace, Microsoft.Extensions.Logging.LogLevel.Error);
            }
            finally
            {
                Data = data.Trim('\0');
            }
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }

        private string CHeckData(NetworkPacketReader reader)
        {
            Index = reader.ReadByte();

            var data = string.Empty;
            var @char = default(char);

            do
            {
                @char = Convert.ToChar(reader.ReadByte());
                data += new string(@char, 1);
            } while (@char != char.Parse("\0"));

            return data;
        }
    }
}