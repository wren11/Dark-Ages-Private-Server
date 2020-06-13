#region

using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Buffs
{
    public class buff_mordion : Buff
    {
        public override string Name => "mor dion";

        public override int Length => 20;
        public override byte Icon => 53;

        public override void OnApplied(Sprite Affected, Buff buff)
        {
            if (Affected is Aisling)
                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, "Your skin turns to stone.");

            Affected.Immunity = true;

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
                    .SendMessage(0x02, "Your skin turns back to flesh.");

            Affected.Immunity = false;

            base.OnEnded(Affected, buff);
        }
    }
}