#region

using Darkages.Network.Game;
using System;
using System.Linq;

#endregion

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat36 : NetworkFormat
    {
        private readonly GameClient _client;

        public ServerFormat36(GameClient client)
        {
            Secured = true;
            Command = 0x36;
            _client = client;
        }

        public enum ClassType : byte
        {
            Guild = 7,
            Monk = 5,
            Peasant = 0,
            Priest = 4,
            Rogue = 2,
            Warrior = 1,
            Wizard = 3
        }

        public enum ListColor : byte
        {
            Brown = 0xA7,
            DarkGray = 0xB7,
            Gray = 0x17,
            Green = 0x80,
            None = 0x00,
            Orange = 0x97,
            Red = 0x04,
            Tan = 0x30,
            Teal = 0x01,
            White = 0x90,
            Clan = 0x54
        }

        public enum StatusIcon : byte
        {
            None,
            Busy,
            Away,
            TeamWanted,
            Team,
            SoloHunting,
            TeamHunting,
            Help
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            ListColor GetUserColor(Aisling user)
            {
                var color = ListColor.White;
                
                if (Math.Abs(_client.Aisling.ExpLevel - user.ExpLevel) < 8)
                    color = ListColor.Orange;
                if (!string.IsNullOrEmpty(user.Clan) && user.Clan == _client.Aisling.Clan)
                    color = ListColor.Clan;

                return color;
            }

            var users = _client.Server.Clients.Where(i => i?.Aisling != null && i.Aisling.LoggedIn)
                .Select(i => i.Aisling).ToArray();

            users = users.OrderByDescending(i => i.MaximumHp + i.MaximumMp * 2).ToArray();

            var count = (ushort) users.Length;
            var total = (short) (users.Length - users.Length / 11);

            writer.Write((ushort) total);
            writer.Write(count);

            foreach (var user in users)
            {
                writer.Write((byte) user.Path);

                var color = GetUserColor(user);

                writer.Write((byte) (
                    user.Serial == _client.Aisling.Serial
                        ? ListColor.Green
                        : color));


                writer.Write((byte) user.ActiveStatus);
                writer.Write((byte) user.Title > 0);
                writer.Write((byte) user.Stage > 0);
                writer.WriteStringA(user.Username);

                /*
                            var p = new ServerPacket(0x36);
            p.WriteUInt16((ushort)userlist.Count());
            p.WriteUInt16((ushort)userlist.Count());
            foreach (Player player in userlist)
            {
                p.WriteByte((byte)player.Class);
                p.WriteByte(255);
                p.WriteByte((byte)player.Status);
                p.WriteByte((byte)player.Title); // title
                p.WriteByte(0x00); // master check
                p.WriteByte(0x01); // emblem icon
                p.WriteUInt16(0x00); // icon 1
                p.WriteUInt16(0x00); // icon 2
                p.WriteString8(player.Name);
            }
            client.Enqueue(p);
                 */
            }
        }
    }
}