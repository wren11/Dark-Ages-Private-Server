using Darkages.Types;

namespace Darkages.Storage.locales.debuffs
{
    public class debuff_blind : Debuff
    {
        public override string Name => "blind";
        public override byte Icon => 114;
        public override int Length => 35;

        public override void OnApplied(Sprite Affected, Debuff debuff)
        {
            if (Affected is Aisling)
            {
                (Affected as Aisling)
                    .Client
                    .SendStats(StatusFlags.StructD);

                (Affected as Aisling)
                    .Client.SendLocation();

                (Affected as Aisling)
                    .Client.SendMessage(0x02, "You are blinded!");
            }

            Affected.SendAnimation(391, Affected, Affected);

            base.OnApplied(Affected, debuff);
        }

        public override void OnDurationUpdate(Sprite Affected, Debuff buff)
        {
            if (Affected is Aisling)
            {
                (Affected as Aisling)
                    .Client
                    .SendStats(StatusFlags.StructD);

                (Affected as Aisling)
                    .Client.SendLocation();
            }

            Affected.SendAnimation(391, Affected, Affected);

            base.OnDurationUpdate(Affected, buff);
        }

        public override void OnEnded(Sprite Affected, Debuff debuff)
        {
            if (Affected is Aisling)
            {
                (Affected as Aisling)
                    .Client
                    .SendStats(StatusFlags.StructD);

                (Affected as Aisling)
                    .Client.SendMessage(0x02, "You can see again.");
            }

            Affected.SendAnimation(379, Affected, Affected);

            base.OnEnded(Affected, debuff);
        }
    }
}