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

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat15 : NetworkFormat
    {
        public ServerFormat15(Area area) : this()
        {
            Area = area;
        }

        public ServerFormat15()
        {
            Secured = true;
            Command = 0x15;
        }

        public Area Area { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write((ushort) Area.ID); // >> 8
            writer.Write((byte) Area.Cols); // W & 0xff
            writer.Write((byte) Area.Rows); // H
            writer.Write((byte) Area.Flags);
            writer.Write(ushort.MinValue);
            writer.Write((ushort) Area.Hash);
            writer.WriteStringA(Area.Name);

            //wb(s, 0) = 170;
            //wb(s, 3) = 21;
            //wb(s, 4) = 0;
            //wb(s, 5) = map->id >> 8;
            //wb(s, 6) = map->id & 0xff;
            //wb(s, 7) = map->xs;
            //wb(s, 8) = map->ys;
            //wb(s, 9) = gsd->account->weather;
            //wb(s, 10) = 0;
            //wb(s, 11) = 0;
            //ws(s, 12) = SWAP16(map->crc);
        }
    }
}