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
        public GameServerTimer Timer = new GameServerTimer(TimeSpan.FromMilliseconds(1000));

        public ObjectComponent(GameServer server) : base(server)
        {
        }

        public override void Update(TimeSpan elapsedTime)
        {
            Timer.Update(elapsedTime);

            if (Timer.Elapsed)
            {
                UpdateObjects();
                Timer.Reset();
            }
        }

        private void UpdateObjects()
        {
            var connectedUsers = Server.Clients.Where(i =>
                i?.Aisling?.Map != null && i.Aisling.Map.Ready).Select(i => i.Aisling).ToArray();

            foreach (var user in connectedUsers) UpdateClientObjects(user);
        }

        public static void UpdateClientObjects(Aisling user)
        {
            var payload = new List<Sprite>();

            if (!user.LoggedIn || !user.Map.Ready)
                return;


            var objects = user
                .GetObjects(user.Map, selector => selector != null && selector.Serial != user.Serial, Get.All)
                .ToArray();
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

            if (payload.Count > 0) user.Show(Scope.Self, new ServerFormat07(payload.ToArray()));
        }

        public static void AddObjects(List<Sprite> payload, Aisling myplayer, Sprite[] objectsToAdd)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            if (myplayer == null) throw new ArgumentNullException(nameof(myplayer));

            foreach (var obj in objectsToAdd)
            {
                if (obj.Serial == myplayer.Serial)
                    continue;

                if (myplayer.View.Contains(obj))
                    continue;

                if (!myplayer.View.Add(obj))
                    continue;

                if (obj is Monster monster)
                {
                    var valueCollection = monster.Scripts?.Values;

                    if (valueCollection != null)
                        foreach (var script in valueCollection)
                            script.OnApproach(myplayer.Client);
                }

                if (obj is Aisling otherplayer)
                {
                    if (!myplayer.Dead && !otherplayer.Dead)
                    {
                        if (myplayer.Invisible)
                            otherplayer.ShowTo(myplayer);
                        else
                            myplayer.ShowTo(otherplayer);

                        if (otherplayer.Invisible)
                            myplayer.ShowTo(otherplayer);
                        else
                            otherplayer.ShowTo(myplayer);
                    }
                    else
                    {
                        if (myplayer.Dead)
                            if (otherplayer.CanSeeGhosts())
                                myplayer.ShowTo(otherplayer);

                        if (otherplayer.Dead)
                            if (myplayer.CanSeeGhosts())
                                otherplayer.ShowTo(myplayer);
                    }
                }
                else
                {
                    var skip = false;

                    switch (obj)
                    {
                        case Mundane sprite:
                        {
                            if (sprite is Mundane mundane)
                            {
                                var template = mundane.Template;


                                if (template.ViewingQualifer.HasFlag(ViewQualifer.Monks))
                                    if (myplayer.ClassID != 5)
                                        skip = true;

                                if (template.ViewingQualifer.HasFlag(ViewQualifer.Warriors))
                                    if (myplayer.ClassID != 1)
                                        skip = true;

                                if (template.ViewingQualifer.HasFlag(ViewQualifer.Rogues))
                                    if (myplayer.ClassID != 2)
                                        skip = true;

                                if (template.ViewingQualifer.HasFlag(ViewQualifer.Wizards))
                                    if (myplayer.ClassID != 3)
                                        skip = true;

                                if (template.ViewingQualifer.HasFlag(ViewQualifer.Priests))
                                    if (myplayer.ClassID != 4)
                                        skip = true;
                            }

                            break;
                        }
                        case Money money:
                        {
                            var goldSetting = myplayer.GameSettings.Find(i =>
                                i.EnabledSettingStr.Contains("AUTO LOOT GOLD"));

                            if (goldSetting != null)
                                if (goldSetting.Enabled)
                                {
                                    money.GiveTo(money.Amount, myplayer);
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
    }
}