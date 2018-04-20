using Darkages.Types;

namespace Darkages.Storage.locales.debuffs
{
    public class debuff_cursed : Debuff
    {
        public debuff_cursed(string name, int length, byte icon)
        {
            Name = name;
            Length = length;
            Icon = icon;
        }

        public virtual StatusOperator AcModifer { get; set; }

        public override void OnApplied(Sprite Affected, Debuff debuff)
        {
            if (Affected is Aisling)
                (Affected as Aisling)
                    .Client
                    .SendStats(StatusFlags.StructD);

            base.OnApplied(Affected, debuff);
        }

        public override void OnDurationUpdate(Sprite Affected, Debuff debuff)
        {
            base.OnDurationUpdate(Affected, debuff);
        }

        public override void OnEnded(Sprite Affected, Debuff debuff)
        {
            if (Affected is Aisling)
            {
                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, string.Format("{0} has ended.", Name));
                (Affected as Aisling)
                    .Client
                    .SendStats(StatusFlags.StructD);
            }


            base.OnEnded(Affected, debuff);
        }
    }
}