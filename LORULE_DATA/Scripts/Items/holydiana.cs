using System;
using Darkages.Scripting;
using Darkages.Types;

namespace Darkages.Storage.locales.Scripts.Items
{
    [Script("Holy Diana", "Dean")]
    public class holydiana : ItemScript
    {
        public holydiana(Item item) : base(item)
        {
        }

        public override void Equipped(Sprite sprite, byte displayslot)
        {
            if ((sprite is Aisling))
            {
                (sprite as Aisling).Weapon = 26;
                Item.ApplyModifers((sprite as Aisling).Client);
            }
        }

        public override void OnUse(Sprite sprite, byte slot)
        {
            if (sprite == null)
                return;
            if (Item == null)
                return;
            if (Item.Template == null)
                return;

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

        public override void UnEquipped(Sprite sprite, byte displayslot)
        {
            if ((sprite is Aisling))
            {
                Item.RemoveModifiers((sprite as Aisling).Client);
                (sprite as Aisling).Weapon = ushort.MinValue;
            }
        }
    }
}
