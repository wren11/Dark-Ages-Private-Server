using Darkages.Types;

namespace Darkages.Storage.locales.Buffs
{
    public class buff_armachd : Buff
    {
        public buff_armachd() : base()
        {
            Name = "armachd";
            Length = 5;
            Icon = 0;
        }

        public StatusOperator AcModifer => new StatusOperator(StatusOperator.Operator.Remove, 10);

        public override void OnApplied(Sprite Affected, Buff buff)
        {
            if (AcModifer.Option == StatusOperator.Operator.Add)
                Affected.BonusAc += (sbyte)AcModifer.Value;
            else if (AcModifer.Option == StatusOperator.Operator.Remove)
                Affected.BonusAc -= (sbyte)AcModifer.Value;

            if (Affected is Aisling)
            {
                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, "Your armor has been increased.");
                (Affected as Aisling)
                        .Client.SendStats(StatusFlags.All);
            }
            base.OnApplied(Affected, buff);
        }

        public override void OnDurationUpdate(Sprite Affected, Buff buff)
        {
            base.OnDurationUpdate(Affected, buff);
        }

        public override void OnEnded(Sprite Affected, Buff buff)
        {
            if (AcModifer.Option == StatusOperator.Operator.Add)
                Affected.BonusAc -= (sbyte)AcModifer.Value;
            else if (AcModifer.Option == StatusOperator.Operator.Remove)
                Affected.BonusAc += (sbyte)AcModifer.Value;

            if (Affected is Aisling)
            {
                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, "Your armor returns to normal.");
                (Affected as Aisling)
                        .Client.SendStats(StatusFlags.All);
            }

            base.OnEnded(Affected, buff);
        }
    }
}