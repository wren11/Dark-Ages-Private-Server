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
using Darkages.Network.ServerFormats;
using Darkages.Types;

namespace Darkages.Storage.locales.Buffs
{
    public class buff_aite : Buff
    {
        /// <summary>
        ///     This name MUST match and correspond the name in the type BUFF.
        /// </summary>
        public override string Name => "aite";
        public override int  Length => 3000;
        public override byte Icon  => 11;

        public buff_aite()
        {

        }

        public override void OnApplied(Sprite Affected, Buff buff)
        {
            if (Affected is Aisling)
                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, "Aite! You are empowered. You glow like gold.");

            base.OnApplied(Affected, buff);
        }

        public override void OnDurationUpdate(Sprite Affected, Buff buff)
        {
            Affected.Show(Scope.NearbyAislings, 
                new ServerFormat29((uint)Affected.Serial,
                (uint)Affected.Serial, 168, 168, 100));

            base.OnDurationUpdate(Affected, buff);
        }

        public override void OnEnded(Sprite Affected, Buff buff)
        {
            if (Affected is Aisling)
                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, "Aite is gone. Your armor returns to normal.");

            base.OnEnded(Affected, buff);
        }
    }
}
