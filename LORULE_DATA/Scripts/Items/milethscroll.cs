using System;
using Darkages.Scripting;
using Darkages.Types;

namespace Darkages.Storage.locales.Scripts.Items
{
    [Script("Mileth Scroll", "Dean")]
    public class milethscroll : ItemScript
    {
        public milethscroll(Item item) : base(item)
        {

        }

        public override void Equipped(Sprite sprite, byte displayslot)
        {

        }

        public override void UnEquipped(Sprite sprite, byte displayslot)
        {

        }

        public override void OnUse(Sprite sprite, byte slot)
        {
            if (sprite is Aisling)
            {
                if (!ServerContext.GlobalWarpTemplateCache.ContainsKey(500) || ServerContext.GlobalWarpTemplateCache[500].Count == 0)
                {
                    (sprite as Aisling).Client.SendMessage(Scope.Self, 0x02, "You can't travel there now.");
                    return;
                }

                (sprite as Aisling).Client.WarpTo(ServerContext.GlobalWarpTemplateCache[509][0]);
                (sprite as Aisling).Client.SendMessage(Scope.Self, 0x02, "You have scrolled to mileth.");
            }
        }
    }
}
