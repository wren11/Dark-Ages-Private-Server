#region

using Darkages.Network.ServerFormats;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Buffs
{
    public class buff_aite : Buff
    {
        public override string Name => "aite";

        public override int Length => 3000;
        public override byte Icon => 11;

        public override void OnApplied(Sprite Affected, Buff buff)
        {
            if (Affected is Aisling)
                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, "Aite! You are empowered. You glow like gold.");

            base.OnApplied(Affected, buff);
        }

        public override void OnDurationUpdate(Sprite Affected, Buff buff)
        {
            Affected.Show(Scope.NearbyAislings,
                new ServerFormat29((uint) Affected.Serial,
                    (uint) Affected.Serial, 168, 168, 100));

            base.OnDurationUpdate(Affected, buff);
        }

        public override void OnEnded(Sprite Affected, Buff buff)
        {
            if (Affected is Aisling)
                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, "Aite is gone. Your armor returns to normal.");

            base.OnEnded(Affected, buff);
        }
    }
}