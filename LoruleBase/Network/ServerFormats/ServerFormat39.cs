///************************************************************************
//Project Lorule: A Dark Ages Client (http://darkages.creatorlink.net/index/)
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

using System;
using System.Linq;
using Darkages.Types;

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

            packet.Write((byte) 0x07);
            packet.Write((byte) 0x0);
            packet.Write((byte) 0x0);
            packet.Write((byte) 0x0);
            packet.Write((byte) 0x0);
            packet.Write((byte) 0x0);
            packet.Write((byte) 0x0);
            packet.Write((byte) 0x0);

            var isGrouped = Aisling.GroupParty != null && (Aisling.GroupParty.PartyMembers != null && (Aisling.GroupParty != null && Aisling.GroupParty.PartyMembers.Count(i => i.Serial != Aisling.Serial) > 0));

            if (!isGrouped)
            {
                packet.WriteStringA("no group");
            }
            else
            {
                var partyMessage = Aisling.GroupParty.PartyMembers.Aggregate(
                    "Group members\n", (current, member) => current + $"{(Aisling.Username.Equals(member.GroupParty.LeaderName, StringComparison.OrdinalIgnoreCase) ? " * " : " ")}{member.Username}\n");

                partyMessage += $"{Aisling.GroupParty.PartyMembers.Count} total";
                packet.WriteStringA(partyMessage);
            }

            packet.Write((byte) Aisling.PartyStatus);
            packet.Write((byte) 0x00);
            packet.Write((byte) Aisling.ClassID);
            packet.Write(Aisling.Nation);
            packet.Write((byte) 0x01);
            packet.WriteStringA(Convert.ToString(Aisling.Stage
                                                 != ClassStage.Class
                ? Aisling.Stage.ToString()
                : Aisling.Path.ToString()));

            packet.WriteStringA(Aisling.Clan);


            var legendSubjects = from subject in Aisling.LegendBook.LegendMarks
                group subject by subject
                into g
                let count = g.Count()
                orderby count descending
                select new
                {
                    Value = Aisling.LegendBook.LegendMarks.Find(i => i.Value == g.Key.Value),
                    Count = Aisling.LegendBook.LegendMarks.Count(i => i.Value == g.Key.Value)
                };


            var exactCount = legendSubjects.Distinct().Count();
            packet.Write((byte) exactCount);

            foreach (var obj in legendSubjects.Distinct().ToList())
            {
                var legend = obj.Value;
                packet.Write(legend.Icon);
                packet.Write(legend.Color);
                packet.WriteStringA(legend.Category);
                packet.WriteStringA($"{legend.Value} {(obj.Count > 1 ? "(" + obj.Count.ToString() + ")" : "")}");
            }

            packet.Write((byte) 0x00);
            packet.Write((ushort) Aisling.Display);
            packet.Write((byte) 0x02);
            packet.Write((uint) 0x00);
            packet.Write((byte) 0x00);
        }
    }
}