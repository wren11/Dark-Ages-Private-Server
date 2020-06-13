#region

using Darkages.Network.ServerFormats;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Buffs
{
    public class buff_hide : Buff
    {
        public override string Name => "Hide";

        public override int Length => 10;
        public override byte Icon => 10;

        public override void OnApplied(Sprite Affected, Buff buff)
        {
            if (Affected is Aisling _aisling)
            {
                var client = (Affected as Aisling).Client;
                if (client.Aisling != null && !client.Aisling.Dead)
                {
                    client.Aisling.Flags = AislingFlags.Invisible;

                    if (client.Aisling.Invisible) client.SendMessage(0x02, "You blend in to the shadows.");

                    var sound = new ServerFormat13
                    {
                        Sound = 43
                    };

                    Affected.Show(Scope.NearbyAislings, sound);
                    client.UpdateDisplay();

                    base.OnApplied(Affected, buff);
                }
            }
        }

        public override void OnDurationUpdate(Sprite Affected, Buff buff)
        {
            Affected.Show(Scope.NearbyAislings,
                new ServerFormat29((uint) Affected.Serial,
                    (uint) Affected.Serial, 0, 0, 100));

            base.OnDurationUpdate(Affected, buff);
        }

        public override void OnEnded(Sprite Affected, Buff buff)
        {
            if (Affected is Aisling)
                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, "You emerge from the shadows.");
            {
                var client = (Affected as Aisling).Client;
                {
                    client.Aisling.Flags ^= AislingFlags.Invisible;
                    client.UpdateDisplay();

                    base.OnEnded(Affected, buff);
                }
            }
        }
    }
}