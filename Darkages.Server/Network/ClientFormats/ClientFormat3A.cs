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


///************************************************************************
namespace Darkages.Network.ClientFormats
{
    public class ClientFormat3A : NetworkFormat
    {
        public ClientFormat3A()
        {
            Secured = true;
            Command = 0x3A;
        }

        public ushort ScriptId { get; set; }
        public ushort Step { get; set; }
        public uint Serial { get; set; }


        public override void Serialize(NetworkPacketReader reader)
        {
            var type = reader.ReadByte();
            var id = reader.ReadUInt32();
            var scriptid = reader.ReadUInt16();
            var step = reader.ReadUInt16();


            ScriptId = scriptid;
            Step = step;
            Serial = id;
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}