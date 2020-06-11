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
using System.Collections.Generic;
using System.Linq;
using Darkages.Network.Object;
using Darkages.Storage;
using Newtonsoft.Json;
using ServiceStack.Text;

namespace Darkages.Types
{
    public class CursedSachel
    {
        public CursedSachel(Aisling parent)
        {
            Owner = parent;
            Items = new HashSet<Item>();
        }

        public ISet<Item> Items { get; set; }
        public string OwnerName { get; set; }
        public DateTime DateReleased { get; set; }
        public Position Location { get; set; }
        public int MapId { get; set; }
        public Item ReaperBag { get; set; }

        [JsonIgnore] public Aisling Owner { get; set; }

        public void GenerateReeper()
        {
            FindOwner();

            if (Owner == null)
                return;

            var itemTemplate = new ItemTemplate
            {
                Name = $"{Owner?.Username}'s Lost Sachel.",
                ScriptName = "Cursed Sachel",
                Image = 135,
                DisplayImage = 0x8000 + 135,
                Flags = ItemFlags.Tradeable | ItemFlags.Consumable | ItemFlags.Stackable | ItemFlags.Dropable,
                Value = 10000000,
                Class = Class.Peasant,
                LevelRequired = 11,
                MaxStack = 255,
                CanStack = true,
                CarryWeight = 1
            };


            ReaperBag = Item.Create(Owner, itemTemplate, true);
            ReaperBag?.Release(Owner, Owner.Position);
        }

        private void FindOwner()
        {
            if (Owner == null) Owner = StorageManager.AislingBucket.Load(OwnerName);
        }

        public void RecoverItems(Aisling Owner)
        {
            FindOwner();

            if (Owner == null)
                return;

            foreach (var item in Items)
            {
                var nitem = ObjectManager.Clone<Item>(item);

                if (nitem.GiveTo(Owner))
                    Owner.Client.SendMessage(0x02, $"You have recovered {item.Template.Name}.");
            }


            Items = new HashSet<Item>();
            {
                Owner.EquipmentManager.RemoveFromInventory(ReaperBag, true);
                Owner.Client.SendStats(StatusFlags.All);
            }

            ReaperBag?.Remove();
            ReaperBag = null;
        }

        public void ReepItems(List<Item> items = null)
        {
            FindOwner();

            if (Owner == null)
                return;

            Items = items != null ? new HashSet<Item>(items) : new HashSet<Item>();
            {
                Location = new Position(Owner.XPos, Owner.YPos);
                MapId = Owner.CurrentMapId;

                ReepInventory();
                ReepEquipment();
                ReepGold();
                GenerateReeper();

                Owner.Client.SendMessage(0x02, ServerContextBase.Config.DeathReepingMessage);
                Owner.Client.SendStats(StatusFlags.All);
            }
        }

        private void ReepGold()
        {
            var gold = Owner.GoldPoints;
            {
                Money.Create(Owner, gold, Owner.Position);
                Owner.GoldPoints = 0;
            }
        }

        private void ReepEquipment()
        {
            List<EquipmentSlot> inv;

            lock (Owner.EquipmentManager.Equipment)
            {
                var batch = Owner.EquipmentManager.Equipment.Where(i => i.Value != null)
                    .Select(i => i.Value);

                inv = new List<EquipmentSlot>(batch);
            }

            foreach (var es in inv)
            {
                var obj = es.Item;

                if (obj?.Template == null)
                    continue;

                if (Owner.EquipmentManager.RemoveFromExisting(es.Slot, false))
                {
                    //reduce item durability.
                    obj.Durability -= obj.Durability * 10 / 100;

                    if (obj.Durability > 0)
                    {
                        var copy = ObjectManager.Clone<Item>(obj);
                        Add(copy, true);
                    }
                }
            }
        }


        private void ReepInventory()
        {
            List<Item> inv;

            lock (Owner.Inventory.Items)
            {
                var batch = Owner.Inventory.Items.Select(i => i.Value);
                inv = new List<Item>(batch);
            }

            foreach (var item in inv)
            {
                var obj = item;

                if (obj?.Template == null)
                    continue;

                obj.Durability -= obj.Durability * 10 / 100;

                //delete the item from inventory.
                Owner.EquipmentManager.RemoveFromInventory(obj, true);

                if (obj.Durability > 0)
                {
                    var copy = ObjectManager.Clone<Item>(obj);
                    Add(copy);
                }
            }
        }

        private void Add(Item obj, bool wasEquipped = false)
        {
            if (obj == null || obj.Template == null)
                return;

            if (wasEquipped)
            {
                if (obj.Template.Flags.HasFlag(ItemFlags.PerishIFEquipped)) return;
            }
            else
            {
                if (obj.Template.Flags.HasFlag(ItemFlags.Perishable)) return;
            }

            lock (Items)
            {
                Items.Add(obj);
            }
        }
    }
}