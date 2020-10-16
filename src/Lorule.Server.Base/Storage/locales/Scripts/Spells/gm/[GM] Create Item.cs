#region

using System;
using Darkages.Scripting;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Spells.gm
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
            var success = false;
            var spellArgs = Arguments ?? throw new ArgumentNullException(nameof(Arguments));

            spellArgs = spellArgs.Trim();

            if (!string.IsNullOrEmpty(spellArgs))
            {
                var exists = ServerContext.GlobalItemTemplateCache.ContainsKey(spellArgs);

                if (exists)
                {
                    var template = ServerContext.GlobalItemTemplateCache[spellArgs];
                    var item = Item.Create(sprite, template);

                    item.Template = template;
                    {
                        item.Release(sprite, sprite.Position);
                        success = true;
                    }
                }

                ServerContext.Logger(
                    $"[GM Create] Used by {(sprite as Aisling).Username} to create {spellArgs}, Success: {success}");
            }
        }
    }
}