using Darkages.Scripting;
using Darkages.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darkages.Storage.locales.Scripts.Spells
{
    [Script("Create", "Dean")]
    public class Create : SpellScript
    {
        public Create(Spell spell) : base(spell)
        {
        }

        public override void OnFailed(Sprite sprite, Sprite target)
        {

        }

        public override void OnSuccess(Sprite sprite, Sprite target)
        {

        }

        public override void OnUse(Sprite sprite, Sprite target)
        {
            var spellArgs = Arguments;
            var Upgrades  = 0;

            if (spellArgs == "die")
            {
                sprite.CurrentHp = 0;
            }

            if (spellArgs.ToLower().Contains("forsaken"))
                Upgrades = 8;
            if (spellArgs.ToLower().Contains("godly"))
                Upgrades = 7;
            if (spellArgs.ToLower().Contains("legendary"))
                Upgrades = 6;
            if (spellArgs.ToLower().Contains("epic"))
                Upgrades = 5;
            if (spellArgs.ToLower().Contains("rare"))
                Upgrades = 4;

            if (Upgrades > 0)
            {
                spellArgs = spellArgs.ToLower().Replace("godly", string.Empty);
                spellArgs = spellArgs.ToLower().Replace("legendary", string.Empty);
                spellArgs = spellArgs.ToLower().Replace("epic", string.Empty);
                spellArgs = spellArgs.ToLower().Replace("rare", string.Empty);
                spellArgs = spellArgs.ToLower().Replace("forsaken", string.Empty);
            }

            spellArgs = spellArgs.Trim();

            if (!string.IsNullOrEmpty(spellArgs))
            {
                var exists = ServerContext.GlobalItemTemplateCache.Keys.FirstOrDefault(i 
                    => i.Equals(spellArgs, StringComparison.OrdinalIgnoreCase));

                if (exists != null)
                {
                    var template = ServerContext.GlobalItemTemplateCache[exists];
                    var offset = template.DisplayImage - 0x8000;
                    var item = Item.Create(sprite, template, false);
                    {
                        item.Upgrades = Upgrades;
                    }

                    Item.ApplyQuality(item);                 
                    item.Release(sprite, sprite.Position);
                }
            }
        }
    }
}
