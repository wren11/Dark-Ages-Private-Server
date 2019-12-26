///************************************************************************
//Project Lorule: A Dark Ages Server (http://darkages.creatorlink.net/index/)
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

using Darkages.Scripting;
using Darkages.Types;

namespace Darkages.Storage.locales.Scripts.Spells
{
    [Script("spell_reflect", "Dean")]
    public class spellreflect : SpellScript
    {
        public spellreflect(Spell spell) : base(spell)
        {
        }

        public override void OnFailed(Sprite sprite, Sprite target)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                client.SendMessage(0x02, "failed.");
            }
        }

        public override void OnSuccess(Sprite sprite, Sprite target)
        {

        }

        public override void OnUse(Sprite sprite, Sprite target)
        {
            if (target is Aisling aobj)
            {

                if (aobj.HasBuff("deireas faileas"))
                {
                    aobj.Client.SendMessage(0x02, "Spells are already being reflected.");
                    return;
                }

                sprite.Aisling(sprite)
                    ?.HasManaFor(Spell)
                    ?.Cast(Spell, target)
                    ?.ApplyBuff("buff_spell_reflect").Cast<Aisling>()?.UpdateStats(Spell)?.TrainSpell(Spell);
            }
            else
            {

            }
        }
    }
}