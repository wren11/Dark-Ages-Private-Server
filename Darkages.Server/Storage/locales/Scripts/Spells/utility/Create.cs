///************************************************************************
//Project Lorule: A Dark Ages Client (http://darkages.creatorlink.net/index/)
//Copyright(C) 2018 TrippyInc Pty Ltd
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
//*************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Darkages.Scripting;
using Darkages.Types;
using ServiceStack.Text;

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