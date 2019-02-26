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

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat37 : NetworkFormat
    {
        public ServerFormat37(Item item, byte slot) : this()
        {
            Item = item;
            EquipmentSlot = slot;
        }

        public ServerFormat37()
        {
            Secured = true;
            Command = 0x37;
        }

        public Item Item { get; set; }
        public byte EquipmentSlot { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(EquipmentSlot);
            writer.Write((ushort)Item.DisplayImage);
            writer.Write((byte)0x03);
            writer.WriteStringA(Item.Template.Name);
            writer.WriteStringA(Item.DisplayName);
            writer.Write(Item.Durability);
            writer.Write(Item.Template.MaxDurability);
        }
    }
}
