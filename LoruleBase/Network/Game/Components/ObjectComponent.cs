#region

using System;
using System.Collections.Generic;
using System.Linq;
using Darkages.Network.ServerFormats;
using Darkages.Types;

#endregion

namespace Darkages.Network.Game.Components
{
    public class ObjectComponent : GameServerComponent
    {
        private readonly GameServerTimer _timer = new GameServerTimer(TimeSpan.FromMilliseconds(100));

        public ObjectComponent(GameServer server) : base(server)
        {
        }

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

        public static void UpdateClientObjects(Aisling user)
        {
            Lorule.Update(() =>
            {
                var payload = new List<Sprite>();

                if (!user.LoggedIn || !user.Map.Ready)
                    return;

                var objects = user.GetObjects(user.Map, selector => selector != null && selector.Serial != user.Serial,
                    Get.All).ToArray();
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
            if (_timer.Update(elapsedTime)) UpdateObjects();
        }

        private void UpdateObjects()
        {
            var connectedUsers = Server.Clients.Where(i =>
                i?.Aisling?.Map != null && i.Aisling.Map.Ready).Select(i => i.Aisling).ToArray();

            foreach (var user in connectedUsers) UpdateClientObjects(user);
        }

        #region Underlying Object Managers
        #endregion
    }
}