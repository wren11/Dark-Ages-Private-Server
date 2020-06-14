#region

using Darkages.Common;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Newtonsoft.Json;
using System;

#endregion

namespace Darkages.Types
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Buff
    {
        public Buff()
        {
            Timer = new GameServerTimer(TimeSpan.FromSeconds(1.0));
        }

        [JsonProperty] public virtual bool Cancelled { get; set; }
        [JsonProperty] public virtual byte Icon { get; set; }
        [JsonProperty] public virtual int Length { get; set; }
        [JsonProperty] public virtual string Name { get; set; }
        [JsonProperty] public GameServerTimer Timer { get; set; }
        [JsonProperty] public virtual int value { get; set; }

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

            (Affected as Aisling)?.Client
                .Send(new ServerFormat3A(Icon, (byte)colorInt));
        }

        public bool Has(string name)
        {
            return Name.Equals(name);
        }

        public virtual void OnApplied(Sprite Affected, Buff buff)
        {
            if (Affected.Buffs.TryAdd(buff.Name, buff)) Display(Affected);
        }

        public virtual void OnDurationUpdate(Sprite Affected, Buff buff)
        {
            Display(Affected);
        }

        public virtual void OnEnded(Sprite Affected, Buff buff)
        {
            if (Affected.Buffs.TryRemove(buff.Name, out var removed))
                (Affected as Aisling)?.Client
                    .Send(new ServerFormat3A(Icon, byte.MinValue));
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
    }
}