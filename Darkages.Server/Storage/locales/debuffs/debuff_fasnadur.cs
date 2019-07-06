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

using Darkages.Types;

namespace Darkages.Storage.locales.debuffs
{
    public class debuff_fasnadur : Debuff
    {
        public override string Name => "fas nadur";
        public override byte Icon => 119;
        public override int Length => 90;

        public override void OnApplied(Sprite Affected, Debuff debuff)
        {
            base.OnApplied(Affected, debuff);

            Affected.Amplified = 1;
        }

        public override void OnDurationUpdate(Sprite Affected, Debuff debuff)
        {
            base.OnDurationUpdate(Affected, debuff);
        }

        public override void OnEnded(Sprite Affected, Debuff debuff)
        {
            Affected.Amplified = 0;

            if (Affected is Aisling)
                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, "You return to normal.");

            base.OnEnded(Affected, debuff);
        }
    }
}