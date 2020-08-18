#region

using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

#endregion

namespace Darkages.Types
{
    public class EquipmentManager
    {
        [JsonIgnore] public GameClient Client { get; set; }

        public EquipmentManager(GameClient client)
        {
            Client = client;
            Equipment = new Dictionary<int, EquipmentSlot>();

            for (byte i = 1; i < 18; i++)
                Equipment[i] = null;
        }

        public EquipmentSlot Belt => this[ItemSlots.Waist];
        public EquipmentSlot Boots => this[ItemSlots.Foot];

        public EquipmentSlot DisplayHelm => this[ItemSlots.Coat];
        public Dictionary<int, EquipmentSlot> Equipment { get; set; }
        public EquipmentSlot Greaves => this[ItemSlots.Leg];
        [JsonIgnore] public int Length => Equipment?.Count ?? 0;
        public EquipmentSlot LGauntlet => this[ItemSlots.LArm];
        public EquipmentSlot LRing => this[ItemSlots.LHand];
        public EquipmentSlot Overcoat => this[ItemSlots.Trousers];
        public EquipmentSlot RGauntlet => this[ItemSlots.RArm];
        public EquipmentSlot RRing => this[ItemSlots.RHand];
        public EquipmentSlot Armor => this[ItemSlots.Armor];
        public EquipmentSlot Earring => this[ItemSlots.Earring];
        public EquipmentSlot FirstAcc => this[ItemSlots.FirstAcc];
        public EquipmentSlot Helmet => this[ItemSlots.Helmet];
        public EquipmentSlot Necklace => this[ItemSlots.Necklace];
        public EquipmentSlot SecondAcc => this[ItemSlots.SecondAcc];
        public EquipmentSlot Shield => this[ItemSlots.Shield];
        public EquipmentSlot Weapon => this[ItemSlots.Weapon];
        public EquipmentSlot this[byte idx] => Equipment.ContainsKey(idx) ? Equipment[idx] : null;

        public void Add(int displayslot, Item item)
        {
            if (Client == null)
                return;

            if (displayslot <= 0 || displayslot > 17)
                return;

            if (item?.Template == null)
                return;

            if (!item.Template.Flags.HasFlag(ItemFlags.Equipable))
                return;

            if (Equipment == null)
                Equipment = new Dictionary<int, EquipmentSlot>();

            if (RemoveFromExisting(displayslot))
                AddEquipment(displayslot, item);
        }

        public void AddEquipment(int displayslot, Item item)
        {
            Equipment[displayslot] = new EquipmentSlot(displayslot, item);

            RemoveFromInventory(item);

            DisplayToEquipment((byte)displayslot, item);

            OnEquipmentAdded((byte)displayslot);
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
                    Client.SendMessage(0x02,
                        $"{item.Template.Name} has broken.");
            }
        }

        public void DisplayToEquipment(byte displayslot, Item item)
        { 
            Client.Send(new ServerFormat37(item, displayslot));
        }

        public bool RemoveFromExisting(int displayslot, bool returnit = true)
        {
            if (Equipment[displayslot] == null)
                return true;

            var itemObj = Equipment[displayslot].Item;

            if (itemObj == null)
                return false;

            RemoveFromSlot(displayslot);

            if (returnit)
                if (itemObj.GiveTo(Client.Aisling, false))
                    return true;

            return HandleUnreturnedItem(itemObj);
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

            return true;
        }

        private bool HandleUnreturnedItem(Item itemObj)
        {
            Client.Aisling.CurrentWeight -= itemObj.Template.CarryWeight;

            if (Client.Aisling.CurrentWeight < 0)
                Client.Aisling.CurrentWeight = 0;

            Client.DelObject(itemObj);
            return true;
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
                    Client.SendMessage(0x02,
                        $"{item.Template.Name} is almost broken!. Please repair it soon (< 10%)");
                    item.Warnings[0] = true;
                }
                else if (p10 <= 30 && p10 > 10 && !item.Warnings[1])
                {
                    Client.SendMessage(0x02,
                        $"{item.Template.Name} is wearing out soon. Please repair it ASAP. (< 30%)");
                    item.Warnings[1] = true;
                }
                else if (p10 <= 50 && p10 > 30 && !item.Warnings[2])
                {
                    Client.SendMessage(0x02, $"{item.Template.Name} will need a repair soon. (< 50%)");
                    item.Warnings[2] = true;
                }
            }
        }

        private void OnEquipmentAdded(byte displayslot)
        {
            foreach (var script in Equipment[displayslot].Item?.Scripts.Values)
                script.Equipped(Client.Aisling, displayslot);

            Equipment[displayslot].Item.Equipped = true;

            Client.SendStats(StatusFlags.All);
            Client.UpdateDisplay();
        }

        private void OnEquipmentRemoved(byte displayslot)
        {
            if (Equipment[displayslot] == null)
                return;

            foreach (var script in Equipment[displayslot].Item?.Scripts.Values)
                script.UnEquipped(Client.Aisling, displayslot);

            Equipment[displayslot].Item.Equipped = false;

            Client.SendStats(StatusFlags.All);
            Client.UpdateDisplay();
        }

        #region Core Methods

        private void RemoveFromSlot(int displayslot)
        {
            Client.Aisling.Show(Scope.Self, new ServerFormat38((byte)displayslot));

            OnEquipmentRemoved((byte)displayslot);

            Equipment[displayslot] = null;
        }

        #endregion
    }
}