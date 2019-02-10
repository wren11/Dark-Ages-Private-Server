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
using Darkages.Common;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Newtonsoft.Json;
using System;

namespace Darkages.Types
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Debuff
    {
        public Debuff()
        {
            Timer = new GameServerTimer(TimeSpan.FromSeconds(1));
        }

        [JsonProperty]
        public virtual string Name { get; set; }

        [JsonProperty]
        public virtual int Length { get; set; }

        [JsonProperty]
        public virtual byte Icon { get; set; }

        [JsonProperty]
        public virtual bool Cancelled { get; set; }

        [JsonIgnore]
        public GameServerTimer Timer { get; set; }

        public bool Has(string name)
        {
            return Name.Equals(name);
        }

        public virtual void OnApplied(Sprite Affected, Debuff debuff)
        {
            if (Affected.Debuffs.TryAdd(debuff.Name, debuff))
            {
                Display(Affected);
            }
        }

        public virtual void OnDurationUpdate(Sprite Affected, Debuff buff)
        {
            Display(Affected);
        }

        public virtual void OnEnded(Sprite Affected, Debuff debuff)
        {
            if (Affected.Debuffs.TryRemove(debuff.Name, out var removed))
            {
                if (Affected is Aisling)
                    (Affected as Aisling)
                        .Client
                        .Send(new ServerFormat3A(Icon, byte.MinValue));
            }
        }

        internal void Update(Sprite Affected, TimeSpan elapsedTime)
        {
            if (Timer.Disabled)
                return;

            Timer.Update(elapsedTime);

            if (Timer.Elapsed)
            {
                if (Length - Timer.Tick > 0)
                    OnDurationUpdate(Affected, this);
                else
                    OnEnded(Affected, this);

                Timer.Tick++;
                Timer.Reset();
            }
        }

        public void Display(Sprite Affected)
        {
            var colorInt = 0;

            if ((Length - Timer.Tick).IsWithin(0, 10))
                colorInt = 1;
            else if ((Length - Timer.Tick).IsWithin(10, 20))
                colorInt = 2;
            else if ((Length - Timer.Tick).IsWithin(20, 30))
                colorInt = 3;
            else if ((Length - Timer.Tick).IsWithin(30, 60))
                colorInt = 4;
            else if ((Length - Timer.Tick).IsWithin(60, 90))
                colorInt = 5;
            else if ((Length - Timer.Tick).IsWithin(90, short.MaxValue))
                colorInt = 6;

            if (Affected is Aisling)
                (Affected as Aisling)
                    .Client
                    .Send(new ServerFormat3A(Icon, (byte)colorInt));
        }
    }
}
