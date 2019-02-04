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
    public class buff_clawfist : Buff
    {
        /// <summary>
        ///     This name MUST match and correspond the name in the type BUFF.
        /// </summary>
        public override string Name
        {
            get
            {
                return "Claw Fist";
            }
        }

        public override int Length
        {
            get
            {
                return 9;
            }
        }
        public override byte Icon
        {
            get
            {
                return 13;
            }
        }

        public buff_clawfist() 
        {

        }

        public override void OnApplied(Sprite Affected, Buff buff)
        {
            if (Affected is Aisling)
                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, "Your hands are empowered!");

            Affected.EmpoweredAssail = true;

            base.OnApplied(Affected, buff);
        }

        public override void OnDurationUpdate(Sprite Affected, Buff buff)
        {
            base.OnDurationUpdate(Affected, buff);
        }

        public override void OnEnded(Sprite Affected, Buff buff)
        {
            if (Affected is Aisling)
                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, "Your hands turn back to normal.");

            Affected.EmpoweredAssail = false;


            base.OnEnded(Affected, buff);
        }
    }
}
