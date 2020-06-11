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
    public class debuff_cursed : Debuff
    {
        public debuff_cursed()
        {

        }

        public debuff_cursed(string name, int length, byte icon)
        {
            Name = name;
            Length = length;
            Icon = icon;
        }

        public virtual StatusOperator AcModifer { get; set; }

        public override void OnApplied(Sprite Affected, Debuff debuff)
        {
            if (Affected is Aisling)
                (Affected as Aisling)
                    .Client
                    .SendStats(StatusFlags.StructD);

            base.OnApplied(Affected, debuff);
        }

        public override void OnDurationUpdate(Sprite Affected, Debuff debuff)
        {
            base.OnDurationUpdate(Affected, debuff);
        }

        public override void OnEnded(Sprite Affected, Debuff debuff)
        {
            if (Affected is Aisling)
            {
                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, string.Format("{0} has ended.", Name));
                (Affected as Aisling)
                    .Client
                    .SendStats(StatusFlags.StructD);
            }


            base.OnEnded(Affected, debuff);
        }
    }
}
