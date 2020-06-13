#region

using Darkages.Network.ServerFormats;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.debuffs
{
    public class debuff_blind : Debuff
    {
        public override string Name => "blind";
        public override byte Icon => 114;
        public override int Length => 7;

        public override void OnApplied(Sprite affected, Debuff debuff)
        {
            if (affected is Aisling aisling)
            {
                aisling.Blind = 1;

                aisling.Map.Flags |= MapFlags.Darkness;
                aisling.Client.Send(new ServerFormat15(affected.Map));
                aisling
                    .Client
                    .SendStats(StatusFlags.StructD);
                aisling
                    .Client.SendMessage(0x02, "You are blinded!");
                aisling
                    .Client.Refresh();
            }

            affected.SendAnimation(391, affected, affected);

            base.OnApplied(affected, debuff);
        }

        public override void OnDurationUpdate(Sprite affected, Debuff buff)
        {
            if (affected is Aisling aisling)
                aisling
                    .Client
                    .SendStats(StatusFlags.StructD);

            affected.SendAnimation(42, affected, affected);

            base.OnDurationUpdate(affected, buff);
        }

        public override void OnEnded(Sprite affected, Debuff debuff)
        {
            if (affected is Aisling aisling)
            {
                aisling.Blind = 0;
                aisling.Map.Flags ^= MapFlags.Darkness;
                aisling.Client.Send(new ServerFormat15(affected.Map));

                aisling
                    .Client
                    .SendStats(StatusFlags.StructD);
                aisling
                    .Client.SendMessage(0x02, "You can see again.");

                aisling
                    .Client.Refresh();
            }

            affected.SendAnimation(379, affected, affected);

            base.OnEnded(affected, debuff);
        }
    }
}