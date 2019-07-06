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

namespace Darkages.Network.ClientFormats
{
    public class ClientFormat43 : NetworkFormat
    {
        public int Serial;
        public byte Type;

        public ClientFormat43()
        {
            Secured = true;
            Command = 0x43;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
            Type = reader.ReadByte();

            if (Type == 0x01)
                Serial = reader.ReadInt32();

            if (Type == 0x02)
            {
                // ???
            }


            if (Type == 0x03)
            {
                // 43 4A 03 00 1A 00 2D 00 00
                X = reader.ReadInt16();
                Y = reader.ReadInt16();
            }
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }

        #region Type 3 Variables

        /// <summary>
        ///     The X component of the coordinate of the tile that the client clicked. (Type 3 Variable)
        /// </summary>
        public short X { get; set; }

        /// <summary>
        ///     The Y component of the coordinate of the tile that the client clicked. (Type 3 Variable)
        /// </summary>
        public short Y { get; set; }

        #endregion
    }
}