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
using Darkages.Network.ServerFormats;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Darkages.Types
{
    public class EquipmentManager
    {
        public EquipmentManager(GameClient _client)
        {
            Client = _client;
            Equipment = new Dictionary<int, EquipmentSlot>();

            for (byte i = 1; i < 18; i++)
                Equipment[i] = null;
        }

        [JsonIgnore] public GameClient Client { get; set; }

        [JsonIgnore]
        public int Length => Equipment?.Count ?? 0;

        public Dictionary<int, EquipmentSlot> Equipment { get; set; }

        public EquipmentSlot this[byte idx] => Equipment.ContainsKey(idx) ? Equipment[idx] : null;

        public EquipmentSlot Weapon => this[ItemSlots.Weapon];

        public EquipmentSlot Armor => this[ItemSlots.Armor];

        public EquipmentSlot Shield => this[ItemSlots.Shield];

        public EquipmentSlot Helmet => this[ItemSlots.Helmet];

        public EquipmentSlot Earring => this[ItemSlots.Earring];

        public EquipmentSlot Necklace => this[ItemSlots.Necklace];

        public EquipmentSlot LRing => this[ItemSlots.LHand];

        public EquipmentSlot RRing => this[ItemSlots.RHand];

        public EquipmentSlot LGauntlet => this[ItemSlots.LArm];

        public EquipmentSlot RGauntlet => this[ItemSlots.RArm];

        public EquipmentSlot Belt => this[ItemSlots.Waist];

        public EquipmentSlot Greaves => this[ItemSlots.Leg];

        public EquipmentSlot Boots => this[ItemSlots.Foot];

        public EquipmentSlot FirstAcc => this[ItemSlots.FirstAcc];

        public EquipmentSlot Overcoat => this[ItemSlots.Trousers];

        public EquipmentSlot DisplayHelm => this[ItemSlots.Coat];

        public EquipmentSlot SecondAcc => this[ItemSlots.SecondAcc];

        private void OnEquipmentRemoved(byte displayslot)
        {
            if (Equipment[displayslot] == null)
                return;

            Equipment[displayslot].Item?.Script?.UnEquipped(Client.Aisling, displayslot);
            Equipment[displayslot].Item.Equipped = false;

            Client.SendStats(StatusFlags.All);
            Client.UpdateDisplay();
        }

        private void OnEquipmentAdded(byte displayslot)
        {
            Equipment[displayslot].Item?.Script?.Equipped(Client.Aisling, displayslot);
            Equipment[displayslot].Item.Equipped = true;

            Client.SendStats(StatusFlags.All);
            Client.UpdateDisplay();
        }

        public void DecreaseDurability()
        {
            var broken = new List<Item>();
            foreach (var equipment in Equipment)
            {
                var item = equipment.Value?.Item;

                if (item?.Template == null)
                    continue;

                if (item.Template.Flags.HasFlag(ItemFlags.Repairable))
                {
                    item.Durability--;
                    {
                        if (item.Durability <= 0)
                            item.Durability = 0;
                    }
                }

                ManageDurabilitySignals(item);

                if (item.Durability == 0 || item.Durability > item.Template.MaxDurability)
                    broken.Add(item);
            }

            foreach (var item in broken)
            {
                if (item?.Template == null)
                    continue;

                if (RemoveFromExisting(item.Template.EquipmentSlot))
                {
                    Client.SendMessage(0x02, 
                        string.Format("{0} has broken.",
                        item.Template.Name));
                }
            }
        }

        private void ManageDurabilitySignals(Item item)
        {
            if (item.Durability > item.Template.MaxDurability)
                item.Template.MaxDurability = item.Durability;

            var p10 = Math.Abs(item.Durability * 100 / item.Template.MaxDurability);

            if (item.Warnings != null)
            {
                if (p10 <= 10 && !item.Warnings[0])
                {
                    Client.SendMessage(0x02, string.Format("{0} is almost broken!. Please repair it soon (< 10%)", item.Template.Name));
                    item.Warnings[0] = true;
                }
                else if (p10 <= 30 && p10 > 10 && !item.Warnings[1])
                {
                    Client.SendMessage(0x02, string.Format("{0} is wearing out soon. Please repair it ASAP. (< 30%)", item.Template.Name));
                    item.Warnings[1] = true;
                }
                else if (p10 <= 50 && p10 > 30 && !item.Warnings[2])
                {
                    Client.SendMessage(0x02, string.Format("{0} will need a repair soon. (< 50%)", item.Template.Name));
                    item.Warnings[2] = true;
                }
            }
        }

        #region Core Methods

        public void Add(int displayslot, Item item)
        {
            if (Client == null)
                return;

            if (displayslot <= 0 || displayslot > 17)
                return;

            if (item == null)
                return;

            if (item.Template == null)
                return;

            if (!item.Template.Flags.HasFlag(ItemFlags.Equipable))
                return;

            if (Equipment == null)
                Equipment = new Dictionary<int, EquipmentSlot>();


            if (RemoveFromExisting(displayslot))
                AddEquipment(displayslot, item);
        }

        public bool RemoveFromExisting(int displayslot, bool returnit = true)
        {
            if (Equipment[displayslot] == null)
                return true;

            //get current equipped item occupying the requested slot.
            var itemObj = Equipment[displayslot].Item;

            //sanity check
            if (itemObj == null)
                return false;

            RemoveFromSlot(displayslot);

            if (returnit)
            {
                if (itemObj.GiveTo(Client.Aisling, false))
                {
                    return true;
                }
            }

            return HandleUnreturnedItem(itemObj);
        }

        private bool HandleUnreturnedItem(Item itemObj)
        {
            Client.Aisling.CurrentWeight -= itemObj.Template.CarryWeight;

            if (Client.Aisling.CurrentWeight < 0)
                Client.Aisling.CurrentWeight = 0;

            Client.DelObject<Item>(itemObj);
            return true;
        }

        private void RemoveFromSlot(int displayslot)
        {
            //send remove equipment packet.
            Client.Aisling.Show(Scope.Self, new ServerFormat38((byte)displayslot));

            OnEquipmentRemoved((byte)displayslot);

            //make sure we remove it!
            Equipment[displayslot] = null;
        }

        public void AddEquipment(int displayslot, Item item)
        {
            Equipment[displayslot] = new EquipmentSlot(displayslot, item);

            //Remove it from inventory, do not handle weight.
            RemoveFromInventory(item, false);

            DisplayToEquipment((byte)displayslot, item);

            OnEquipmentAdded((byte)displayslot);
        }

        public void DisplayToEquipment(byte displayslot, Item item)
        {
            //Send Equipment packet.
            Client.Send(new ServerFormat37(item, displayslot));
        }

        public bool RemoveFromInventory(Item item, bool handleWeight = false)
        {
            if (Client.Aisling.Inventory.Remove(item.Slot) != null)
            {
                Client.Send(new ServerFormat10(item.Slot));

                if (handleWeight)
                {
                    Client.Aisling.CurrentWeight -= item.Template.CarryWeight;
                    if (Client.Aisling.CurrentWeight < 0)
                        Client.Aisling.CurrentWeight = 0;

                    Client.SendStats(StatusFlags.StructA);
                }

                Client.LastItemDropped = item;
                return true;
            }

            return false;
        }

        #endregion
    }
}
