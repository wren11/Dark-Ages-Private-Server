#region

using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.debuffs
{
    public class debuff_fasspoirad : Debuff
    {
        public override string Name => "fas spiorad";
        public override byte Icon => 26;
        public override int Length => 2;

        public StatusOperator AcModifer => new StatusOperator(Operator.Remove, 50);

        public override void OnApplied(Sprite Affected, Debuff debuff)
        {
            base.OnApplied(Affected, debuff);

            if (AcModifer.Option == Operator.Add)
                Affected.BonusAc += AcModifer.Value;
        }

        public override void OnDurationUpdate(Sprite Affected, Debuff debuff)
        {
            base.OnDurationUpdate(Affected, debuff);
        }

        public override void OnEnded(Sprite Affected, Debuff debuff)
        {
            if (AcModifer.Option == Operator.Add)
                Affected.BonusAc -= AcModifer.Value;

            base.OnEnded(Affected, debuff);
        }
    }
}