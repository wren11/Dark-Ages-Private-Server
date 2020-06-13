#region

using Darkages.Network.ServerFormats;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.debuffs
{
    public class debuff_hurricane : Debuff
    {
        public override string Name => "hurricane";
        public override byte Icon => 116;
        public override int Length => 1;

        public StatusOperator AcModifer => new StatusOperator(Operator.Add, 30);

        public override void OnApplied(Sprite Affected, Debuff debuff)
        {
            base.OnApplied(Affected, debuff);

            if (AcModifer.Option == Operator.Add)
                Affected.BonusAc += AcModifer.Value;

            if (Affected is Aisling)
            {
                (Affected as Aisling)
                    .Client
                    .SendAnimation(265,
                        (Affected as Aisling).Client.Aisling,
                        (Affected as Aisling).Client.Aisling.Target ??
                        (Affected as Aisling).Client.Aisling);

                var hpbar = new ServerFormat13
                {
                    Serial = Affected.Serial,
                    Health = 255,
                    Sound = 64
                };

                (Affected as Aisling).Show(Scope.Self, hpbar);
            }
            else
            {
                var nearby = Affected.GetObjects<Aisling>(Affected.Map, i => i.WithinRangeOf(Affected));

                foreach (var near in nearby)
                    near.Client.SendAnimation(226, Affected, Affected);
            }
        }

        public override void OnDurationUpdate(Sprite Affected, Debuff debuff)
        {
            if (Affected is Aisling)
            {
                (Affected as Aisling)
                    .Client.SendLocation();

                (Affected as Aisling)
                    .Client.SendAnimation(269,
                        (Affected as Aisling).Client.Aisling,
                        (Affected as Aisling).Client.Aisling.Target ??
                        (Affected as Aisling).Client.Aisling);

                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, "Your armor feels light...");
            }
            else
            {
                var nearby = Affected.GetObjects<Aisling>(Affected.Map, i => Affected.WithinRangeOf(i));

                foreach (var near in nearby)
                {
                    if (near == null || near.Client == null)
                        continue;

                    if (Affected == null)
                        continue;

                    var client = near.Client;
                    client.SendAnimation(269, Affected, client.Aisling);
                }
            }

            base.OnDurationUpdate(Affected, debuff);
        }

        public override void OnEnded(Sprite Affected, Debuff debuff)
        {
            if (Affected is Aisling)
                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, "The hurricane has passed.");

            if (AcModifer.Option == Operator.Add)
                Affected.BonusAc -= AcModifer.Value;

            base.OnEnded(Affected, debuff);
        }
    }
}