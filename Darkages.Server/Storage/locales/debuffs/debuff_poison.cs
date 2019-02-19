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
using System.Linq;

namespace Darkages.Storage.locales.debuffs
{
    public class Debuff_poison : Debuff
    {
        public ushort Animation { get; set; }
        public double Modifier  { get; set; }
        public bool IsSpreading { get; set;  }

        public Debuff_poison()
        {

        }

        public Debuff_poison(string name, int length, byte icon, ushort animation, double mod = 0.05, bool spread = false)
        {
            Animation = animation;
            Name = name;
            Length = length;
            Icon = icon;
            Modifier = mod;
            IsSpreading = spread;
        }

        public override void OnApplied(Sprite Affected, Debuff debuff)
        {
            base.OnApplied(Affected, debuff);

            if (Affected is Aisling)
            {
                (Affected as Aisling)
                    .Client
                    .SendAnimation(Animation,
                        (Affected as Aisling).Client.Aisling,
                        (Affected as Aisling).Client.Aisling.Target ??
                        (Affected as Aisling).Client.Aisling);
            }
            else
            {
                var nearby = Affected.GetObjects<Aisling>(Affected.Map, i => i.WithinRangeOf(Affected));

                foreach (var near in nearby)
                    near.Client.SendAnimation(Animation, Affected, Affected);
            }
        }

        public override void OnDurationUpdate(Sprite Affected, Debuff debuff)
        {
            if (Affected is Aisling)
            {
                (Affected as Aisling)
                    .Client.SendAnimation(Animation,
                        (Affected as Aisling).Client.Aisling,
                        (Affected as Aisling).Client.Aisling.Target ??
                        (Affected as Aisling).Client.Aisling);

                ApplyPoison(Affected);

                (Affected as Aisling).Client.SendStats(StatusFlags.StructB);

                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, "You are infected with poison.");
            }
            else
            {
                ApplyPoison(Affected);

                var nearby = Affected.GetObjects<Aisling>(Affected.Map, i => Affected.WithinRangeOf(i));

                foreach (var near in nearby)
                {
                    if (near == null || near.Client == null)
                        continue;

                    if (Affected == null)
                        continue;

                    var client = near.Client;
                    client.SendAnimation(Animation, Affected, client.Aisling);
                }


            }

            base.OnDurationUpdate(Affected, debuff);
        }

        private void ApplyPoison(Sprite Affected)
        {

            if (IsSpreading)
            {
                var nearby = (from v in Affected.MonstersNearby()
                              where v.Serial != Affected.Serial &&
                              !v.HasDebuff(this.Name)
                              select v).ToList();

                if (nearby.Count > 0)
                {
                    foreach (var near in nearby)
                    {
                        if (near.Target == null && Affected.Target != null)
                        {
                            near.Target = Affected.Target;
                        }

                        OnApplied(near, new Debuff_poison("Poison Trap", Length, Icon, Animation, 0.35, false));
                    }
                }
            }


            if (Modifier <= 0.0)
                Modifier = 0.3;

            if (Affected.CurrentHp > 0)
            {
                var cap = (int)(Affected.CurrentHp - (Affected.CurrentHp * Modifier));
                if (cap > 0)
                {
                    Affected.CurrentHp = cap;
                }
            }
        }

        public override void OnEnded(Sprite Affected, Debuff debuff)
        {
            if (Affected is Aisling)
                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, "you feel better now.");

            base.OnEnded(Affected, debuff);
        }
    }
}
