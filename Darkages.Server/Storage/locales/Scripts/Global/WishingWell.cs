using Darkages.Common;
using Darkages.Network.Game;
using Darkages.Scripting;
using Darkages.Types;
using System;
using System.Linq;

namespace Darkages.Storage.locales.Scripts.Global
{
    [Script("WishingWell", "Dean")]
    public class WishingWell : GlobalScript
    {
        public WishingWell(GameClient client) : base(client)
        {
            Timer = new GameServerTimer(TimeSpan.FromMilliseconds(200));
        }

        public override void Run(GameClient client)
        {
            if (Client.Aisling.AreaID == 1001)
            {
                var item = Client.LastItemDropped;
                if (item != null)
                {
                    if (item.X == 31 && item.Y == 52 || item.X == 31 && item.Y == 53)
                    {

                        if (Client.Aisling.EquipmentManager.RemoveFromInventory(item, true))
                        {
                            var valReqarded = item.Template.Value + 15 / 3;
                            Client.SendMessage(Scope.Self, 0x02, string.Format("Thank you, You are rewarded with {0} Experience!", valReqarded));

                            item.Remove();

                            Client.Aisling.Animate(205);

                            lock (Generator.Random)
                            {
                                var n = Generator.Random.Next(0, ServerContext.GlobalBuffCache.Count - 1);
                                var buffs = ServerContext.GlobalBuffCache.Keys.ToArray();

                                Client.Aisling.ApplyBuff(buffs[n]);
                            }
                            Client.LastItemDropped = null;
                        }
                    }
                }
            }
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (Timer != null)
            {
                Timer.Update(elapsedTime);

                if (Timer.Elapsed)
                {
                    Run(Client);
                    Timer.Reset();
                }
            }
        }
    }
}
