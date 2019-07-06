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

namespace Darkages.Storage.locales.Buffs
{
    public class buff_armachd : Buff
    {
        public buff_armachd()
        {
            Name = "armachd";
            Length = 60;
            Icon = 0;
        }

        public StatusOperator AcModifer => new StatusOperator(Operator.Remove, 25);

        public override void OnApplied(Sprite Affected, Buff buff)
        {
            if (AcModifer.Option == Operator.Add)
                Affected.BonusAc += AcModifer.Value;
            else if (AcModifer.Option == Operator.Remove)
                Affected.BonusAc -= AcModifer.Value;

            if (Affected is Aisling)
            {
                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, "Your armor has been increased.");
                (Affected as Aisling)
                    .Client.SendStats(StatusFlags.All);
            }

            base.OnApplied(Affected, buff);
        }

        public override void OnDurationUpdate(Sprite Affected, Buff buff)
        {
            base.OnDurationUpdate(Affected, buff);
        }

        public override void OnEnded(Sprite Affected, Buff buff)
        {
            if (AcModifer.Option == Operator.Add)
                Affected.BonusAc -= AcModifer.Value;
            else if (AcModifer.Option == Operator.Remove)
                Affected.BonusAc += AcModifer.Value;

            if (Affected is Aisling)
            {
                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, "Your armor returns to normal.");
                (Affected as Aisling)
                    .Client.SendStats(StatusFlags.All);
            }

            base.OnEnded(Affected, buff);
        }
    }
}