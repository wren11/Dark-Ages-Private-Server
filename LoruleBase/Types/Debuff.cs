#region

using System;
using Darkages.Common;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Newtonsoft.Json;

#endregion

namespace Darkages.Types
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Debuff
    {
        public Debuff()
        {
            Timer = new GameServerTimer(TimeSpan.FromSeconds(1));
        }

        [JsonProperty] public ushort Animation { get; set; }
        [JsonProperty] public virtual bool Cancelled { get; set; }
        [JsonProperty] public virtual byte Icon { get; set; }
        [JsonProperty] public virtual int Length { get; set; }
        [JsonProperty] public virtual string Name { get; set; }
        [JsonProperty] public GameServerTimer Timer { get; set; }

        public void Display(Sprite affected)
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

            (affected as Aisling)?.Client
                .Send(new ServerFormat3A(Icon, (byte)colorInt));
        }

        public bool Has(string name)
        {
            return Name.Equals(name);
        }

        public virtual void OnApplied(Sprite affected, Debuff debuff)
        {
            if (affected.Debuffs.TryAdd(debuff.Name, debuff)) Display(affected);
        }

        public virtual void OnDurationUpdate(Sprite affected, Debuff buff)
        {
            Display(affected);
        }

        public virtual void OnEnded(Sprite affected, Debuff debuff)
        {
            if (affected.Debuffs.TryRemove(debuff.Name, out var removed))
                (affected as Aisling)?.Client
                    .Send(new ServerFormat3A(Icon, byte.MinValue));
        }

        internal void Update(Sprite affected, TimeSpan elapsedTime)
        {
            if (Timer.Disabled)
                return;

            if (Timer.Update(elapsedTime))
            {
                if (Length - Timer.Tick > 0)
                    OnDurationUpdate(affected, this);
                else
                    OnEnded(affected, this);

                Timer.Tick++;
            }
        }
    }
}