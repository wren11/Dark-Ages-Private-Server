using Darkages.Types;

namespace Darkages.Storage.locales.debuffs
{
    public class debuff_cradh : debuff_cursed
    {
        public debuff_cradh() : base("cradh", 120, 82)
        {
        }

        public override StatusOperator AcModifer => new StatusOperator(StatusOperator.Operator.Add, 30);

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