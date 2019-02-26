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
using Darkages.Types;
using System;

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat39 : NetworkFormat
    {
        public ServerFormat39(Aisling aisling) : this()
        {
            Aisling = aisling;
        }

        public ServerFormat39()
        {
            Secured = true;
            Command = 0x39;
        }

        public Aisling Aisling { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter packet)
        {
            packet.Write(Aisling.Nation);
            packet.WriteStringA(Aisling.ClanRank);

            packet.Write((byte)0x07);
            packet.Write((byte)0x0);
            packet.Write((byte)0x0);
            packet.Write((byte)0x0);
            packet.Write((byte)0x0);
            packet.Write((byte)0x0);
            packet.Write((byte)0x0);
            packet.Write((byte)0x0);

            var isGrouped = Aisling.GroupParty.LengthExcludingSelf > 0;

            if (!isGrouped)
            {
                packet.WriteStringA("no group");
            }
            else
            {
                var partyMessage = "Group members\n";

                foreach (var member in Aisling.GroupParty.Members)
                    partyMessage += string.Format("{0}{1}\n",
                        Aisling.Username.Equals(member.GroupParty.Creator.Username,
                            StringComparison.OrdinalIgnoreCase)
                            ? " * "
                            : " ", member.Username);
                partyMessage += string.Format("{0} total", Aisling.GroupParty.Length);
                packet.WriteStringA(partyMessage);
            }

            packet.Write((byte)Aisling.PartyStatus);
            packet.Write((byte)0x00);
            packet.Write((byte)Aisling.ClassID);
            packet.Write((byte)Aisling.Nation);
            packet.Write((byte)0x01);
            packet.WriteStringA(Convert.ToString(Aisling.Stage
                                                 != ClassStage.Class
                ? Aisling.Stage.ToString()
                : Aisling.Path.ToString()));

            packet.WriteStringA(Aisling.Clan);

            packet.Write((byte)Aisling.LegendBook.LegendMarks.Count);
            foreach (var legend in Aisling.LegendBook.LegendMarks)
            {
                packet.Write(legend.Icon);
                packet.Write(legend.Color);
                packet.WriteStringA(legend.Category);
                packet.WriteStringA(legend.Value + string.Format(" - {0}", DateTime.UtcNow.ToShortDateString()));
            }

            packet.Write((byte)0x00);
            packet.Write((ushort)Aisling.Display);
            packet.Write((byte)0x02);
            packet.Write((uint)0x00);
            packet.Write((byte)0x00);
        }
    }
}
