#region

using Darkages.Network.Object;
using Darkages.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Darkages.Types
{
    public class CursedSachel
    {
        public CursedSachel(Aisling parent)
        {
            Owner = parent;
            Items = new HashSet<Item>();
        }

        public DateTime DateReleased { get; set; }
        public ISet<Item> Items { get; set; }
        public Position Location { get; set; }
        public int MapId { get; set; }
        [JsonIgnore] public Aisling Owner { get; set; }
        public string OwnerName { get; set; }
        public Item ReaperBag { get; set; }

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

                Owner.Client.SendMessage(0x02, ServerContext.Config.DeathReepingMessage);
                Owner.Client.SendStats(StatusFlags.All);
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

        private void FindOwner()
        {
            if (Owner == null)
                Owner = StorageManager.AislingBucket.Load(OwnerName);
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
                    obj.Durability -= obj.Durability * 10 / 100;

                    if (obj.Durability > 0)
                    {
                        var copy = ObjectManager.Clone<Item>(obj);
                        Add(copy, true);
                    }
                }
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

                Owner.EquipmentManager.RemoveFromInventory(obj, true);

                if (obj.Durability > 0)
                {
                    var copy = ObjectManager.Clone<Item>(obj);
                    Add(copy);
                }
            }
        }
    }
}