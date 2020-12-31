#region

using Darkages.Network.ServerFormats;
using Darkages.Types;
using System;
using System.Collections.Generic;
using System.Linq;

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

                    if (monster.Template != null && monster.Map != null)
                        Monster.InitScripting(monster.Template, monster.Map, monster);

                    if (valueCollection != null && valueCollection.Any())
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

                if (obj.Summoner != null)
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

                if (user != null && (!user.LoggedIn || !user.Map.Ready))
                    return;

                if (user != null)
                {
                    var objects = user.GetObjects(user.Map,
                        selector => selector != null && selector.Serial != user.Serial,
                        Get.All).ToArray();
                    var objectsInView = objects.Where(s => s != null && s.WithinRangeOf(user)).ToArray();
                    var objectsNotInView = objects.Where(s => s != null && !s.WithinRangeOf(user)).ToArray();
                    var objectsToRemove = objectsNotInView.Except(objectsInView).ToList();
                    var objectsToAdd = objectsInView.Except(objectsNotInView).ToArray();

                    CheckObjectClients(user, objects);

                    RemoveObjects(
                        user,
                        objectsToRemove.ToArray());

                    AddObjects(payload,
                        user,
                        objectsToAdd);


                    if (payload.Count > 0)
                        user.Show(Scope.Self, new ServerFormat07(payload.ToArray()));
                }
            });
        }

        private static void CheckObjectClients(Aisling user, Sprite[] objects)
        {
            foreach (var obj in objects)
                if (obj is Aisling aisling)
                    lock (ServerContext.SyncLock)
                    {
                        if (ServerContext.Game != null && ServerContext.Game.Clients != null)
                        {
                            var clients = ServerContext.Game.Clients.FindAll(i =>
                                i?.Aisling != null && string.Equals(i.Aisling.Username, aisling.Username,
                                    StringComparison.CurrentCultureIgnoreCase));

                            if (clients.Count == 0)
                            {
                                user.HideFrom(aisling);
                                aisling.HideFrom(user);
                                user.DelObject(aisling);
                            }
                        }
                    }
        }

        protected internal override void Update(TimeSpan elapsedTime)
        {
            if (_timer.Update(elapsedTime)) UpdateObjects();
        }

        private void UpdateObjects()
        {
            var connectedUsers = Server.Clients.Where(i =>
                i?.Aisling?.Map != null && i.Aisling.Map.Ready).Select(i => i.Aisling).ToArray();

            foreach (var user in connectedUsers)
                UpdateClientObjects(user);
        }

        #region Underlying Object Managers

        #endregion
    }
}