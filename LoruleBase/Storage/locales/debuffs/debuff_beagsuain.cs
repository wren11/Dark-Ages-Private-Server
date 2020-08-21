#region

using Darkages.Network.ServerFormats;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.debuffs
{
    public class debuff_beagsuain : Debuff
    {
        public override byte Icon => 38;
        public override int Length => 5;
        public override string Name => "beag suain";

        public override void OnApplied(Sprite affected, Debuff debuff)
        {
            base.OnApplied(affected, debuff);

            if (affected is Aisling)
            {
                (affected as Aisling)
                    .Client
                    .SendAnimation(41,
                        (affected as Aisling).Client.Aisling,
                        (affected as Aisling).Client.Aisling.Target ??
                        (affected as Aisling).Client.Aisling);

                var hpbar = new ServerFormat13
                {
                    Serial = affected.Serial,
                    Health = 255,
                    Sound = 64
                };

                (affected as Aisling).Show(Scope.Self, hpbar);
            }
            else
            {
                var nearby = affected.GetObjects<Aisling>(affected.Map, i => i.WithinRangeOf(affected));

                foreach (var near in nearby)
                    near.Client.SendAnimation(41, affected, affected);
            }
        }

        public override void OnDurationUpdate(Sprite affected, Debuff debuff)
        {
            if (affected is Aisling)
            {
                (affected as Aisling)
                    .Client.SendLocation();

                (affected as Aisling)
                    .Client.SendAnimation(41,
                        (affected as Aisling).Client.Aisling,
                        (affected as Aisling).Client.Aisling.Target ??
                        (affected as Aisling).Client.Aisling);

                (affected as Aisling)
                    .Client
                    .SendMessage(0x02, "You've been incapacitated.");
            }
            else
            {
                var nearby = affected.GetObjects<Aisling>(affected.Map, i => affected.WithinRangeOf(i));

                foreach (var near in nearby)
                {
                    if (near?.Client == null)
                        continue;

                    var client = near.Client;
                    client.SendAnimation(41, affected, client.Aisling);
                }
            }

            base.OnDurationUpdate(affected, debuff);
        }

        public override void OnEnded(Sprite affected, Debuff debuff)
        {
            if (affected is Aisling)
                (affected as Aisling)
                    .Client
                    .SendMessage(0x02, "Your are free again.");

            base.OnEnded(affected, debuff);
        }
    }
}