using Darkages.Types;

namespace Darkages.Storage.locales.debuffs
{
    public class debuff_fasnadur : Debuff
    {
        public override string Name => "fas nadur";
        public override byte Icon => 119;
        public override int Length => 90;

        public override void OnApplied(Sprite Affected, Debuff debuff)
        {
            base.OnApplied(Affected, debuff);

            Affected.Amplified = 1;
        }

        public override void OnDurationUpdate(Sprite Affected, Debuff debuff)
        {
            base.OnDurationUpdate(Affected, debuff);
        }

        public override void OnEnded(Sprite Affected, Debuff debuff)
        {
            Affected.Amplified = 0;

            if (Affected is Aisling)
                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, "You return to normal.");

            base.OnEnded(Affected, debuff);
        }
    }
}