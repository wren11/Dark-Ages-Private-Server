using Darkages.Types;

namespace Darkages.Storage.locales.debuffs
{
    public class debuff_ardcradh : debuff_cursed
    {
        public debuff_ardcradh() : base("ard cradh", 480, 84)
        {
        }

        public override StatusOperator AcModifer => new StatusOperator(StatusOperator.Operator.Add, 50);

        public override void OnApplied(Sprite Affected, Debuff debuff)
        {
            if (AcModifer.Option == StatusOperator.Operator.Add)
                Affected.BonusAc += (sbyte) AcModifer.Value;

            base.OnApplied(Affected, debuff);
        }

        public override void OnEnded(Sprite Affected, Debuff debuff)
        {
            if (AcModifer.Option == StatusOperator.Operator.Add)
                Affected.BonusAc -= (sbyte) AcModifer.Value;

            base.OnEnded(Affected, debuff);
        }
    }
}