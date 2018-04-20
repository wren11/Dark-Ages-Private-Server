using Darkages.Scripting;
using Darkages.Types;
using static Darkages.Types.ElementManager;

namespace Darkages.Storage.locales.Scripts.Items
{
    [Script("Belt", "Dean")]
    public class Belt : ItemScript
    {
        public Belt(Item item) : base(item)
        {
        }

        public override void Equipped(Sprite sprite, byte displayslot)
        {
            if (Item.Template.Flags.HasFlag(ItemFlags.Elemental))
                if (Item.DefenseElement != Element.None)
                    sprite.DefenseElement = Item.Template.DefenseElement;

            Item?.ApplyModifers((sprite as Aisling).Client);
            (sprite as Aisling).Client.SendStats(StatusFlags.StructD);
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
            if (Item.Template.Flags.HasFlag(ItemFlags.Elemental))
                sprite.DefenseElement = Element.None;

            (sprite as Aisling).Client.SendStats(StatusFlags.StructD);
            Item?.RemoveModifiers((sprite as Aisling).Client);
        }
    }
}