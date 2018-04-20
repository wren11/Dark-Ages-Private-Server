using Darkages.Network.ServerFormats;
using Darkages.Types;

namespace Darkages.Storage.locales.debuffs
{
    public class debuff_sleep : Debuff
    {
        public override string Name => "sleep";
        public override byte Icon => 90;
        public override int Length => 8;

        public override void OnApplied(Sprite Affected, Debuff debuff)
        {
            base.OnApplied(Affected, debuff);

            if (Affected is Aisling)
            {
                (Affected as Aisling)
                    .Client
                    .SendAnimation(32,
                        (Affected as Aisling).Client.Aisling,
                        (Affected as Aisling).Client.Aisling.Target ??
                        (Affected as Aisling).Client.Aisling);
            }
            else
            {
                var nearby = Affected.GetObjects<Aisling>(i => i.WithinRangeOf(Affected));

                foreach (var near in nearby)
                    near.Client.SendAnimation(32, Affected, Affected);
            }
        }

        public override void OnDurationUpdate(Sprite Affected, Debuff debuff)
        {
            if (Affected is Aisling)
            {
                (Affected as Aisling)
                    .Client.SendLocation();

                (Affected as Aisling)
                    .Client.SendAnimation(32,
                        (Affected as Aisling).Client.Aisling,
                        (Affected as Aisling).Client.Aisling.Target ??
                        (Affected as Aisling).Client.Aisling);

                var hpbar = new ServerFormat13
                {
                    Serial = Affected.Serial,
                    Health = 255,
                    Sound = 8
                };

                (Affected as Aisling).Show(Scope.NearbyAislings, hpbar);

                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, "You have been put to sleep.");
            }
            else
            {
                var nearby = Affected.GetObjects<Aisling>(i => Affected.WithinRangeOf(i));

                foreach (var near in nearby)
                {
                    if (near == null || near.Client == null)
                        continue;

                    if (Affected == null)
                        continue;

                    var client = near.Client;
                    client.SendAnimation(32, Affected, client.Aisling);
                }
            }

            base.OnDurationUpdate(Affected, debuff);
        }

        public override void OnEnded(Sprite Affected, Debuff debuff)
        {
            if (Affected is Aisling)
                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, "awake!");

            base.OnEnded(Affected, debuff);
        }
    }
}