using Darkages.Network.Game;
using System;

namespace Darkages.Types
{
    public class Pet : Summon
    {
        private readonly GameClient _client;

        public Pet(GameClient client) : base(client)
        {
            _client = client;
        }

        // addition pet logic can go here, such as TP back to player.
        public override void UpdateSpawns(TimeSpan elapsedTime)
        {
            KeepSpawnsNearSummoner();

            void KeepSpawnsNearSummoner()
            {
                foreach (var (_, spawn) in Spawns)
                {
                    var aisling = spawn.Summoner;

                    if (aisling == null)
                        Despawn();

                    if (aisling == null || aisling.WithinRangeOf(spawn, 9, true)) 
                        continue;

                    var sprite = GetObject(null, i => i.Serial == spawn.Serial, Get.All);

                    //warp to our summoner.
                    if (sprite != null)
                    {
                        sprite.X = aisling.X;
                        sprite.Y = aisling.Y;
                        sprite.CurrentMapId = aisling.CurrentMapId;
                        sprite.Direction = aisling.Direction;
                        sprite.Update();
                    }
                }
            }
        }
    }
}