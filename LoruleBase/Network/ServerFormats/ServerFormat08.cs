#region

using Darkages.Types;

#endregion

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat08 : NetworkFormat
    {
        public ServerFormat08(Aisling aisling, StatusFlags flags) : this()
        {
            Aisling = aisling;
            Flags = (byte)flags;
        }

        public ServerFormat08()
        {
            Secured = true;
            Command = 0x08;
        }

        public Aisling Aisling { get; set; }
        public byte Flags { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            if (Aisling.GameMaster)
                Flags |= 0x40;
            else
                Flags |= 0x40 | 0x80;

            writer.Write(Flags);

            var hp = Aisling.MaximumHp >= int.MaxValue || Aisling.MaximumHp <= 0 ? 1 : Aisling.MaximumHp;
            var mp = Aisling.MaximumMp >= int.MaxValue || Aisling.MaximumMp <= 0 ? 1 : Aisling.MaximumMp;

            var chp = Aisling.CurrentHp >= int.MaxValue || Aisling.CurrentHp <= 0 ? 1 : Aisling.CurrentHp;
            var cmp = Aisling.CurrentMp >= int.MaxValue || Aisling.CurrentMp <= 0 ? 1 : Aisling.CurrentMp;

            if ((Flags & 0x20) != 0)
            {
                writer.Write((byte)1);
                writer.Write((byte)0);
                writer.Write((byte)0);

                writer.Write((byte)Aisling.ExpLevel);
                writer.Write((byte)Aisling.AbpLevel);

                writer.Write((uint)hp);
                writer.Write((uint)mp);

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
                writer.Write((uint)chp);
                writer.Write((uint)cmp);
            }

            if ((Flags & 0x08) != 0)
            {
                writer.Write(Aisling.ExpTotal);
                writer.Write((uint)Aisling.ExpLevel >= ServerContext.Config.PlayerLevelCap
                    ? 0
                    : Aisling.ExpNext);
                writer.Write((uint)Aisling.AbpTotal);
                writer.Write((uint)Aisling.AbpNext);
                writer.Write((uint)Aisling.GamePoints);
                writer.Write((uint)Aisling.GoldPoints);
            }

            if ((Flags & 0x04) != 0)
            {
                writer.Write(uint.MinValue);
                writer.Write(Aisling.Blind);
                writer.Write((byte)0x00);
                writer.Write((byte)Aisling.OffenseElement);
                writer.Write((byte)Aisling.DefenseElement);
                writer.Write((byte)(Aisling.Mr / 10));
                writer.Write(byte.MinValue);
                writer.Write((sbyte)Aisling.Ac);
                writer.Write(Aisling.Dmg);
                writer.Write(Aisling.Hit);
            }
        }
    }
}