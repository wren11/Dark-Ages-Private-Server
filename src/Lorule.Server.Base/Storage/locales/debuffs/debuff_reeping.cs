#region

using System;
using Darkages.Network.ServerFormats;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.debuffs
{
    public class debuff_reeping : Debuff
    {
        public readonly Random _rnd = new Random();
        public int Count => Messages.Length;
        public override byte Icon => 89;
        public override int Length => ServerContext.Config.SkullLength;

        public string[] Messages =>
            ServerContext.Config.ReapMessage.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);

        public override string Name => "skulled";

        public override void OnApplied(Sprite Affected, Debuff debuff)
        {
            /* GM Character's don't die. */
            if (Affected is Aisling)
                if ((Affected as Aisling).GameMaster)
                    return;

            base.OnApplied(Affected, debuff);

            if (Affected.CurrentMapId == ServerContext.Config.DeathMap)
            {
                base.OnEnded(Affected, debuff);
                return;
            }

            if (Affected is Aisling)
            {
                (Affected as Aisling)
                    .Client
                    .SendAnimation(24,
                        (Affected as Aisling).Client.Aisling,
                        (Affected as Aisling).Client.Aisling.Target ??
                        (Affected as Aisling).Client.Aisling);

                var hpbar = new ServerFormat13
                {
                    Serial = Affected.Serial,
                    Health = 255,
                    Sound = 6
                };

                (Affected as Aisling).Show(Scope.Self, hpbar);
            }
            else
            {
                var nearby = Affected.GetObjects<Aisling>(Affected.Map, i => i.WithinRangeOf(Affected));

                foreach (var near in nearby)
                    near.Client.SendAnimation(24, Affected, Affected);
            }
        }

        public override void OnDurationUpdate(Sprite Affected, Debuff debuff)
        {
            if (Affected.CurrentMapId == ServerContext.Config.DeathMap)
            {
                base.OnEnded(Affected, debuff);
                return;
            }

            if (Affected is Aisling)
            {
                (Affected as Aisling)
                    .Client.SendAnimation(24,
                        (Affected as Aisling).Client.Aisling,
                        (Affected as Aisling).Client.Aisling.Target ??
                        (Affected as Aisling).Client.Aisling);

                var hpbar = new ServerFormat13
                {
                    Serial = Affected.Serial,
                    Health = 255,
                    Sound = 6
                };

                (Affected as Aisling).Show(Scope.Self, hpbar);

                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, Messages[_rnd.Next(Count) % Messages.Length]);
            }
            else
            {
                var nearby = Affected.GetObjects<Aisling>(Affected.Map, i => Affected.WithinRangeOf(i));

                foreach (var near in nearby)
                {
                    if (near == null || near.Client == null)
                        continue;

                    if (Affected == null)
                        continue;

                    var client = near.Client;
                    client.SendAnimation(24, Affected, client.Aisling);
                }
            }

            base.OnDurationUpdate(Affected, debuff);
        }

        public override void OnEnded(Sprite Affected, Debuff debuff)
        {
            if (Affected.CurrentMapId == ServerContext.Config.DeathMap)
            {
                base.OnEnded(Affected, debuff);
                return;
            }

            if (Affected is Aisling && !debuff.Cancelled)
            {
                (Affected as Aisling)
                    .Client
                    .SendMessage(0x02, "You have died.");

                var hpbar = new ServerFormat13
                {
                    Serial = Affected.Serial,
                    Health = 255,
                    Sound = 5
                };

                (Affected as Aisling).Show(Scope.Self, hpbar);
                (Affected as Aisling).Flags = AislingFlags.Ghost;
                (Affected as Aisling).CastDeath();
                (Affected as Aisling).SendToHell();
            }

            base.OnEnded(Affected, debuff);
        }
    }
}