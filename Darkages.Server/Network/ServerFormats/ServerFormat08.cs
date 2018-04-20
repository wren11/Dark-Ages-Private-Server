using Darkages.Types;

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat08 : NetworkFormat
    {
        public ServerFormat08(Aisling aisling, StatusFlags flags)
        {
            Aisling = aisling;
            Flags = (byte)flags;
        }

        public override bool Secured => true;

        public override byte Command => 0x08;

        public Aisling Aisling { get; set; }
        public byte Flags { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Flags);

            if ((Flags & 0x20) != 0)
            {
                writer.Write((byte)1);
                writer.Write((byte)0);
                writer.Write((byte)0);

                writer.Write((byte)Aisling.ExpLevel);
                writer.Write((byte)Aisling.AbpLevel);

                writer.Write((uint)Aisling.MaximumHp);
                writer.Write((uint)Aisling.MaximumMp);

                writer.Write(Aisling.Str);
                writer.Write(Aisling.Int);
                writer.Write(Aisling.Wis);
                writer.Write(Aisling.Con);
                writer.Write(Aisling.Dex);

                if (Aisling.StatPoints > 0)
                {
                    writer.Write((byte)1);
                    writer.Write((byte)Aisling.StatPoints);
                }
                else
                {
                    writer.Write((byte)0);
                    writer.Write((byte)0);
                }


                writer.Write((ushort)Aisling.MaximumWeight);
                writer.Write((ushort)Aisling.CurrentWeight);
                writer.Write(uint.MinValue);
            }

            if ((Flags & 0x10) != 0)
            {
                writer.Write((uint)Aisling.CurrentHp);
                writer.Write((uint)Aisling.CurrentMp);
            }

            if ((Flags & 0x08) != 0)
            {
                writer.Write((uint)Aisling.ExpTotal);
                writer.Write((uint)Aisling.ExpNext);
                writer.Write((uint)Aisling.AbpTotal);
                writer.Write((uint)Aisling.AbpNext);
                writer.Write((uint)Aisling.GamePoints);
                writer.Write((uint)Aisling.GoldPoints);
            }

            if ((Flags & 0x04) != 0)
            {
                writer.Write(uint.MinValue);
                writer.Write(Aisling.Blind);
                writer.Write((byte)0x10);
                writer.Write((byte)Aisling.OffenseElement); // element off
                writer.Write((byte)Aisling.DefenseElement); // element def
                writer.Write((byte)(Aisling.Mr / 10));
                writer.Write(byte.MinValue);
                writer.Write((sbyte)Aisling.Ac);
                writer.Write(Aisling.Dmg);
                writer.Write(Aisling.Hit);
            }
        }
    }
}