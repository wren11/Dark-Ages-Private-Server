using System;
using System.Text;

namespace Darkages.Network.ClientFormats
{
    public class ClientFormat39 : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x39;

        public byte Type { get; set; }
        public int Serial { get; set; }
        public ushort Step { get; set; }
        public string Args { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Type = reader.ReadByte();
            Serial = reader.ReadInt32();
            Step = reader.ReadUInt16();

            if (reader.CanRead)
            {
                var length = reader.ReadByte();

                if (Step == 0x0500 || Step == 0x0800 || Step == 0x9000)
                {
                    Args = Convert.ToString(length);
                }
                else
                {
                    var data = reader.ReadBytes(length);
                    Args = Encoding.ASCII.GetString(data);
                }
            }
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}