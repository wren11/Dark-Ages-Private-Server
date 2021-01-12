#region

using Darkages.Scripting;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Items
{
    [Script("Shield", "Dean")]
    public class Shield : ItemScript
    {
        public Shield(Item item) : base(item)
        {
        }

        public override void Equipped(Sprite sprite, byte displayslot)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                if (Item.Template == null)
                    return;

                Item.ApplyModifers(client);

                client.Aisling.Shield = (byte) Item.Template.Image;
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
                var obj = sprite as Aisling;
                if (obj.EquipmentManager.Weapon != null
                    && obj.EquipmentManager.Weapon.Item.Template.Flags.HasFlag(ItemFlags.TwoHanded))
                    if (!obj.EquipmentManager.RemoveFromExisting(obj.EquipmentManager.Weapon.Slot))
                    {
                        obj.Client.SendMessage(0x02, "You require both hands to equip such an item.");
                        return;
                    }
            }

            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                if (Item.Template.Flags.HasFlag(ItemFlags.Equipable))
                    if (client.CheckReqs(client, Item))
                        client.Aisling.EquipmentManager.Add(Item.Template.EquipmentSlot, Item);
            }
        }

        public override void UnEquipped(Sprite sprite, byte displayslot)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                if (Item.Template == null)
                    return;

                client.Aisling.Shield = byte.MinValue;

                Item.RemoveModifiers(client);
            }
        }
    }
}