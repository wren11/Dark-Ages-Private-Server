#region

using Darkages.Scripting;
using Darkages.Types;
using System;
using System.Linq;

#endregion

namespace Darkages.Storage.locales.Scripts.Spells
{
    [Script("[GM] Create Item", "Dean")]
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
            var spellArgs = Arguments ?? throw new ArgumentNullException(nameof(Arguments));

            if (spellArgs == "die") sprite.CurrentHp = 0;

            if (spellArgs == "+hit") sprite._Hit += 10;

            spellArgs = spellArgs.Trim();

            if (!string.IsNullOrEmpty(spellArgs))
            {
                var exists = ServerContextBase.GlobalItemTemplateCache.Keys.FirstOrDefault(i
                    => i.Equals(spellArgs, StringComparison.OrdinalIgnoreCase));

                if (exists != null)
                {
                    var template = ServerContextBase.GlobalItemTemplateCache[exists];
                    var offset = template.DisplayImage - 0x8000;
                    var item = Item.Create(sprite, template);

                    item.Template = template;
                    {
                        item.Release(sprite, sprite.Position);
                    }
                }
            }
        }
    }
}