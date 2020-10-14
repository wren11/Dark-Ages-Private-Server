#region

using Darkages.Scripting;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Items
{
    [Script("Weapon", "Dean")]
    public class Weapon : ItemScript
    {
        public Weapon(Item item) : base(item)
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

                client.Aisling.Weapon = Item.Template.Image;
                client.Aisling.UsingTwoHanded = Item.Template.Flags.HasFlag(ItemFlags.TwoHanded);
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

            if (Item.Template.Flags.HasFlag(ItemFlags.TwoHanded)
                && sprite is Aisling)
            {
                var obj = sprite as Aisling;
                if (obj.EquipmentManager.Shield != null)
                    if (!obj.EquipmentManager.RemoveFromExisting(obj.EquipmentManager.Shield.Slot))
                    {
                        obj.Client.SendMessage(0x02, "You require both hands to equip such an item.");
                        return;
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