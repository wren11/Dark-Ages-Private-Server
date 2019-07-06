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

using System;
using System.Text;

namespace Darkages.Network.ClientFormats
{
    public class ClientFormat39 : NetworkFormat
    {
        public ClientFormat39()
        {
            Secured = true;
            Command = 0x39;
        }

        public byte Type { get; set; }
        public int Serial { get; set; }
        public ushort Step { get; set; }
        public string Args { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Type = reader.ReadByte();
            Serial = reader.ReadInt32();
            Step = reader.ReadUInt16();

            if (reader.GetCanRead())
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