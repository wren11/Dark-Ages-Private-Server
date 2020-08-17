#region

using Darkages.Network.ServerFormats;
using Darkages.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

#endregion

namespace Darkages.Network.Game.Components
{
    public class ObjectComponent : GameServerComponent
    {
        public static Dictionary<int, Thread> WorkerThreads = new Dictionary<int, Thread>();
        public GameServerTimer Timer = new GameServerTimer(TimeSpan.FromMilliseconds(100));
        private static DateTime _lastFrameUpdate = DateTime.UtcNow;

        public ObjectComponent(GameServer server) : base(server)
        {
        }

        #region Underlying Object Managers

        public static void AddObjects(List<Sprite> payload, Aisling player, Sprite[] objectsToAdd)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            if (player == null) throw new ArgumentNullException(nameof(player));

            foreach (var obj in objectsToAdd)
            {
                if (obj.Serial == player.Serial)
                    continue;

                if (player.View.Contains(obj))
                    continue;

                if (!player.View.Add(obj))
                    continue;

                if (obj is Monster monster)
                {
                    var valueCollection = monster.Scripts?.Values;

                    if (valueCollection != null)
                        foreach (var script in valueCollection)
                            script.OnApproach(player.Client);
                }

                if (obj is Aisling otherplayer)
                {
                    if (!player.Dead && !otherplayer.Dead)
                    {
                        if (player.Invisible)
                            otherplayer.ShowTo(player);
                        else
                            player.ShowTo(otherplayer);

                        if (otherplayer.Invisible)
                            player.ShowTo(otherplayer);
                        else
                            otherplayer.ShowTo(player);
                    }
                    else
                    {
                        if (player.Dead)
                            if (otherplayer.CanSeeGhosts())
                                player.ShowTo(otherplayer);

                        if (otherplayer.Dead)
                            if (player.CanSeeGhosts())
                                otherplayer.ShowTo(player);
                    }
                }
                else
                {
                    var skip = false;

                    switch (obj)
                    {
                        case Money money:
                            {
                                var goldSetting = player.GameSettings.Find(i =>
                                    i.EnabledSettingStr.Contains("AUTO LOOT GOLD"));

                                if (goldSetting != null)
                                    if (goldSetting.Enabled)
                                    {
                                        money.GiveTo(money.Amount, player);
                                        skip = true;
                                    }

                                break;
                            }
                    }

                    if (!skip)
                        payload.Add(obj);
                }
            }
        }

        public void BeginUpdatingMap(Area area)
        {
            if (!WorkerThreads.ContainsKey(area.ID))
            {
                WorkerThreads[area.ID] = new Thread(() => UpdateAreaObjects(area))
                {
                    IsBackground = true
                };
                WorkerThreads[area.ID].Start();
            }
        }

        public static void RemoveObjects(Aisling client, Sprite[] objectsToRemove)
        {
            if (objectsToRemove == null)
                return;

            foreach (var obj in objectsToRemove)
            {
                if (obj.Serial == client.Serial)
                    continue;

                if (!client.View.Contains(obj))
                    continue;

                if (!client.View.Remove(obj))
                    continue;

                if (obj is Monster monster)
                {
                    var valueCollection = monster.Scripts?.Values;

                    if (valueCollection != null)
                        foreach (var script in valueCollection)
                            script.OnLeave(client.Client);
                }

                obj.HideFrom(client);
            }
        }

        #endregion

        public void UpdateAreaObjects(Area area)
        {
            bool Leave() => !area.GetObjects(area, i => i?.Map != null && i.Map.Ready, Get.Aislings).Any();

            var frameRate = TimeSpan.FromSeconds(1.0 / 30);

            while (true)
            {
                if (!area.Ready)
                    continue;

                if (Leave()) break;

                var elapsedTime = DateTime.UtcNow - _lastFrameUpdate;

                static bool Predicate(Sprite n) => true;

                var objectCache = area.GetObjects(area, Predicate, Get.Items | Get.Money | Get.Monsters | Get.Mundanes);

                if (objectCache == null)
                    return;

                foreach (var obj in objectCache)
                {
                    switch (obj)
                    {
                        case Aisling aisling:
                            break;

                        case Monster monster when monster.Map == null || monster.Scripts == null:
                            continue;
                        case Monster monster:
                            {
                                if (obj.CurrentHp <= 0x0 && obj.Target != null && !monster.Skulled)
                                {
                                    foreach (var script in monster.Scripts.Values.Where(
                                        script => obj.Target?.Client != null))
                                        script?.OnDeath(obj.Target.Client);

                                    monster.Skulled = true;
                                }

                                foreach (var script in monster.Scripts.Values)
                                    script?.Update(elapsedTime);

                                if (obj.TrapsAreNearby())
                                {
                                    var nextTrap = Trap.Traps.Select(i => i.Value)
                                        .FirstOrDefault(i => i.Location.X == obj.X && i.Location.Y == obj.Y);

                                    if (nextTrap != null)
                                        Trap.Activate(nextTrap, obj);
                                }

                                monster.UpdateBuffs(elapsedTime);
                                monster.UpdateDebuffs(elapsedTime);
                                monster.LastUpdated = DateTime.UtcNow;
                                break;
                            }
                        case Item item:
                            {
                                var stale = !((DateTime.UtcNow - item.AbandonedDate).TotalMinutes > 3);

                                if (item.Cursed && stale)
                                {
                                    item.AuthenticatedAislings = null;
                                    item.Cursed = false;
                                }

                                break;
                            }
                        case Money money:
                            break;

                        case Mundane mundane:
                            {
                                if (mundane.CurrentHp <= 0)
                                    mundane.CurrentHp = mundane.Template.MaximumHp;

                                mundane.UpdateBuffs(elapsedTime);
                                mundane.UpdateDebuffs(elapsedTime);
                                mundane.Update(elapsedTime);
                                break;
                            }
                    }

                    obj.LastUpdated = DateTime.UtcNow;
                }

                _lastFrameUpdate = DateTime.UtcNow;
                Thread.Sleep(frameRate);
            }

            if (WorkerThreads.ContainsKey(area.ID))
            {
                WorkerThreads.Remove(area.ID);
            }
        }

        public static void UpdateClientObjects(Aisling user)
        {
            Lorule.Update(() =>
            {
                var payload = new List<Sprite>();

                if (!user.LoggedIn || !user.Map.Ready)
                    return;

                var objects = user.GetObjects(user.Map, selector => selector != null && selector.Serial != user.Serial, Get.All).ToArray();
                var objectsInView = objects.Where(s => s.WithinRangeOf(user)).ToArray();
                var objectsNotInView = objects.Where(s => !s.WithinRangeOf(user)).ToArray();
                var objectsToRemove = objectsNotInView.Except(objectsInView).ToArray();
                var objectsToAdd = objectsInView.Except(objectsNotInView).ToArray();

                RemoveObjects(
                    user,
                    objectsToRemove);

                AddObjects(payload,
                    user,
                    objectsToAdd);

                if (payload.Count > 0)
                    user.Show(Scope.Self, new ServerFormat07(payload.ToArray()));
            });
        }

        public override void Update(TimeSpan elapsedTime)
        {
            Timer.Update(elapsedTime);

            if (!Timer.Elapsed)
                return;

            UpdateObjects();
            Timer.Reset();
        }

        private void UpdateObjects()
        {
            var connectedUsers = Server.Clients.Where(i =>
                i?.Aisling?.Map != null && i.Aisling.Map.Ready).Select(i => i.Aisling).ToArray();

            foreach (var user in connectedUsers)
            {
                BeginUpdatingMap(user.Map);
                UpdateClientObjects(user);
            }
        }
    }
}