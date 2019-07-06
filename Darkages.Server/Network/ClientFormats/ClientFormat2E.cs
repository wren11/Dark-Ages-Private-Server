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
    public class ClientFormat2E : NetworkFormat
    {
        public ClientFormat2E()
        {
            Secured = true;
            Command = 0x2E;
        }

        public byte Type { get; set; }
        public string Name { get; set; }
        public bool ShowOnMap { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Type = reader.ReadByte();

            if (Type == 0x02)
                Name = reader.ReadStringA();

            if (Type == 0x08)
            {
                Name = reader.ReadStringA();
                ShowOnMap = reader.ReadBool();
            }
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}