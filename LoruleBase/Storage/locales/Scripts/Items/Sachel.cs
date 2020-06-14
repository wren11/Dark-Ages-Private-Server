#region

using Darkages.Scripting;
using Darkages.Types;
using System;

#endregion

namespace Darkages.Storage.locales.Scripts.Items
{
    [Script("Cursed Sachel")]
    public class Sachel : ItemScript
    {
        public Sachel(Item item) : base(item)
        {
        }

        public override void Equipped(Sprite sprite, byte displayslot)
        {
        }

        public override void OnUse(Sprite sprite, byte slot)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                var name = Item.Template.Name.Replace("'s Lost Sachel.", string.Empty);

                if (name == string.Empty)
                {
                    client.SendMessage(0x02, ServerContextBase.Config.CantUseThat);
                    return;
                }

                if (name.Trim().Equals(client.Aisling.Username, StringComparison.OrdinalIgnoreCase))
                {
                    if (client.Aisling.Remains.ReaperBag != null) client.Aisling.Remains.RecoverItems(client.Aisling);
                }
                else
                {
                    sprite._MaximumHp += 50;
                    client.SendStats(StatusFlags.All);

                    client.SendMessage(Scope.All, 0x02,
                        $"{client.Aisling.Username} broke open {name}'s Cursed Sachel. (Granted +50 hp!)");
                }
            }
        }

        public override void UnEquipped(Sprite sprite, byte displayslot)
        {
        }
    }
}