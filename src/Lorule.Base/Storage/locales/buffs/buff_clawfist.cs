#region

using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Buffs
{
    public class buff_clawfist : Buff
    {
        public override byte Icon => 13;
        public override int Length => 9;
        public override string Name => "Claw Fist";

        public override void OnApplied(Sprite Affected, Buff buff)
        {
            if (Affected is Aisling)
                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, "Your hands are empowered!");

            Affected.EmpoweredAssail = true;

            base.OnApplied(Affected, buff);
        }

        public override void OnDurationUpdate(Sprite Affected, Buff buff)
        {
            base.OnDurationUpdate(Affected, buff);
        }

        public override void OnEnded(Sprite Affected, Buff buff)
        {
            if (Affected is Aisling)
                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, "Your hands turn back to normal.");

            Affected.EmpoweredAssail = false;

            base.OnEnded(Affected, buff);
        }
    }
}