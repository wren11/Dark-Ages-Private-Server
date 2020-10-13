#region

using Darkages.Network.Game;
using System;
using System.Linq;
using Darkages.Types;
using ServiceStack;

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

        [Flags]
        public enum ClassType : byte
        {
            Peasant = 0x00,
            Warrior = 0x01,
            Rogue   = 0x02,
            Wizard  = 0x03,
            Priest  = 0x04,
            Monk    = 0x05,
            Guild   = 0x80
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
            Clan = 0x54,
            Me = 0x70
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

        public static int n = 0x00;

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
                var color = GetUserColor(user);

                var path = ((byte)ClassType.Guild | n);

                writer.Write((byte) path);
                writer.Write((byte) color);
                writer.Write((byte) user.ActiveStatus);
                writer.Write((byte) user.Title > 0);
                writer.Write((byte) user.Stage > 0);
                writer.WriteStringA(user.Username);


                Console.WriteLine(string.Format("0x{0:X2} = {1},", n, path));
            }

            n++;
        }
    }
}