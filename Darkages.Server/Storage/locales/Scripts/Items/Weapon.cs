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
using Darkages.Scripting;
using Darkages.Types;

namespace Darkages.Storage.locales.Scripts.Items
{
    [Script("Weapon", "Dean")]
    public class Weapon : ItemScript
    {
        public Weapon(Item item) : base(item)
        {
        }

        public override void OnUse(Sprite sprite, byte slot)
        {
            if (sprite == null)
                return;
            if (Item == null)
                return;
            if (Item.Template == null)
                return;

            if (Item.Template.Flags.HasFlag(ItemFlags.TwoHanded)
                && sprite is Aisling)
            {
                var obj = sprite as Aisling;
                if (obj.EquipmentManager.Shield != null)
                {
                    if (!obj.EquipmentManager.RemoveFromExisting(obj.EquipmentManager.Shield.Slot, true))
                    {
                        obj.Client.SendMessage(0x02, "You require both hands to equip such an item.");
                        return;
                    }
                }
            }

            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                if (Item.Template.Flags.HasFlag(ItemFlags.Equipable))
                {
                    if (!client.CheckReqs(client, Item))
                    {
                    }
                }
                else
                {
                    client.Aisling.EquipmentManager.Add(Item.Template.EquipmentSlot, Item);
                }
            }
        }


        public override void Equipped(Sprite sprite, byte displayslot)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                if (Item.Template == null)
                    return;

                Item.ApplyModifers(client);

                client.Aisling.Weapon = Item.Template.Image;
                client.Aisling.UsingTwoHanded = Item.Template.Flags.HasFlag(ItemFlags.TwoHanded);
            }
        }

        public override void UnEquipped(Sprite sprite, byte displayslot)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                if (Item.Template == null)
                    return;

                client.Aisling.Weapon = ushort.MinValue;
                client.Aisling.UsingTwoHanded = false;

                Item.RemoveModifiers(client);
            }
        }
    }
}
