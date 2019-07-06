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
    /// <summary>
    ///     Class Beagioc.
    ///     Implements the <see cref="Darkages.Scripting.SpellScript" />
    /// </summary>
    /// <seealso cref="Darkages.Scripting.SpellScript" />
    [Script("beag ioc", "Dean")]
    public class Beagioc : SpellScript
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Beagioc" /> class.
        /// </summary>
        /// <param name="spell">The spell.</param>
        public Beagioc(Spell spell) : base(spell)
        {
        }

        /// <summary>
        ///     Called when [failed].
        /// </summary>
        /// <param name="sprite">The sprite.</param>
        /// <param name="target">The target.</param>
        public override void OnFailed(Sprite sprite, Sprite target)
        {
        }

        /// <summary>
        ///     Called when [success].
        /// </summary>
        /// <param name="sprite">The sprite.</param>
        /// <param name="target">The target.</param>
        public override void OnSuccess(Sprite sprite, Sprite target)
        {
        }


        /// <summary>
        ///     Called when [use].
        /// </summary>
        /// <param name="sprite">The sprite.</param>
        /// <param name="target">The target.</param>
        public override void OnUse(Sprite sprite, Sprite target)
        {
            var healValue = (int) (200 + Spell.Level * sprite.Wis * 0.05);

            sprite.Aisling(sprite)
                ?.HasManaFor(Spell)
                ?.Cast(Spell, target)
                ?.GiveHealth(target, healValue)
                ?.UpdateStats(Spell)
                ?.TrainSpell(Spell);
        }
    }
}