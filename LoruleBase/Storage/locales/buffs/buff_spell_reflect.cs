#region

using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Buffs
{
    public class buff_spell_reflect : Buff
    {
        public override string Name => "deireas faileas";

        public override byte Icon => 54;

        public override int Length => 12;

        public override void OnApplied(Sprite Affected, Buff buff)
        {
            Affected.SpellReflect = true;

            base.OnApplied(Affected, buff);
        }

        public override void OnDurationUpdate(Sprite Affected, Buff buff)
        {
            base.OnDurationUpdate(Affected, buff);
        }

        public override void OnEnded(Sprite Affected, Buff buff)
        {
            Affected.SpellReflect = false;


            if (Affected is Aisling)
                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, "Spells attacking you now stop reflecting.");

            base.OnEnded(Affected, buff);
        }
    }
}