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
    public class buff_hide : Buff
    {
        /// <summary>
        ///     This name MUST match and correspond the name in the type BUFF.
        /// </summary>
        public override string Name => "Hide";
        public override int Length => 10;
        public override byte Icon => 10;

        public buff_hide()
        {

        }

        public override void OnApplied(Sprite Affected, Buff buff)
        {
            if (Affected is Aisling _aisling)
            {
                var client = (Affected as Aisling).Client;
                if (client.Aisling != null && !client.Aisling.Dead)
                {
                    client.Aisling.Flags = AislingFlags.Invisible;

                    if (client.Aisling.Invisible)
                    {
                        client.SendMessage(0x02, "You blend in to the shadows.");
                    }

                    var sound = new ServerFormat13
                    {
                        Sound = 43
                    };

                    Affected.Show(Scope.NearbyAislings, sound);
                    client.UpdateDisplay();

                    base.OnApplied(Affected, buff);
                }
            }
        }

        public override void OnDurationUpdate(Sprite Affected, Buff buff)
        {
            Affected.Show(Scope.NearbyAislings,
                new ServerFormat29((uint)Affected.Serial,
                (uint)Affected.Serial, 0, 0, 100));

            base.OnDurationUpdate(Affected, buff);
        }

        public override void OnEnded(Sprite Affected, Buff buff)
        {
            if (Affected is Aisling)
                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, "You emerge from the shadows.");
            {
                var client = (Affected as Aisling).Client;
                {
                    client.Aisling.Flags ^= AislingFlags.Invisible;
                    client.UpdateDisplay();

                    base.OnEnded(Affected, buff);
                }
            }
        }
    }
}
