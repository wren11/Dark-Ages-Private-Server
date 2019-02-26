///************************************************************************
//Project Lorule: A Dark Ages Server (http://darkages.creatorlink.net/index/)
//Copyright(C) 2018 TrippyInc Pty Ltd
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
//*************************************************************************/
using Darkages.Network.Game;
using System;
using System.Linq;

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat36 : NetworkFormat
    {
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
            White = 0x90
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

        public ServerFormat36(GameClient client)
        {
            Secured = true;
            Command = 0x36;
            Client  = client;
        }

        private GameClient Client;

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            var users = Client.Server.Clients.Where(i => i != null && i.Aisling != null && i.Aisling.LoggedIn).Select(i => i.Aisling).ToArray();
            users = users.OrderByDescending(i => i.MaximumHp + i.MaximumMp * 2).ToArray();

            var count = (ushort)users.Length;
            var total = (short)(users.Length - users.Length / 11);

            writer.Write((ushort)total);
            writer.Write(count);

            foreach (var user in users)
            {
                writer.Write((byte)user.ClassID);
                writer.Write((byte)(
                    user.Serial == Client.Aisling.Serial
                        ? ListColor.Tan
                        : Math.Abs(Client.Aisling.ExpLevel - user.ExpLevel) < 10
                            ? ListColor.Orange
                            : ListColor.White));
                writer.Write((byte)user.ActiveStatus);
                writer.Write((byte)user.Title > 0);
                writer.Write((byte)user.Stage > 0);
                writer.WriteStringA(user.Username);
            }
        }
    }
}
