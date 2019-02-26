namespace Darkages.Network.Game.Components
{
    using Darkages.Network.ServerFormats;
    using Darkages.Types;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class ObjectComponent : GameServerComponent
    {
        public GameServerTimer Timer = new GameServerTimer(TimeSpan.FromMilliseconds(50));

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
            var connected_users = Server.Clients.Where(i =>
                i != null &&
                i.Aisling != null &&
                i.Aisling.Map != null &&
                i.Aisling.Map.Ready).Select(i => i.Aisling).ToArray();

            for (int i = 0; i < connected_users.Length; i++)
            {
                var user = connected_users[i];

                UpdateClientObjects(user);
            }
        }

        public static void UpdateClientObjects(Aisling user)
        {
            var payload = new List<Sprite>();

            if (user.LoggedIn && user.Map.Ready)
            {
                var objects           = user.GetObjects(user.Map, selector => selector != null && selector.Serial != user.Serial, Get.All);
                var objectsInView     = objects.Where(s => s.WithinRangeOf(user));
                var objectsNotInView  = objects.Where(s => !s.WithinRangeOf(user));
                var ObjectsToRemove   = objectsNotInView.Except(objectsInView).ToArray();
                var ObjectsToAdd      = objectsInView.Except(objectsNotInView).ToArray();

                RemoveObjects(
                    user,
                    ObjectsToRemove);

                AddObjects(payload,
                    user,
                    ObjectsToAdd);

                if (payload.Count > 0)
                {
                    user.Show(Scope.Self, new ServerFormat07(payload.ToArray()));
                }
            }
        }

        public static void AddObjects(List<Sprite> payload, Aisling client, Sprite[] ObjectsToAdd)
        {
            foreach (var obj in ObjectsToAdd)
            {
                if (obj.Serial == client.Serial)
                    continue;

                if (!client.View.Contains(obj))
                {
                    if (client.View.Add(obj))
                    {
                        if (obj is Monster)
                        {
                            (obj as Monster).Script?.OnApproach(client.Client);
                        }

                        if (obj is Aisling)
                            obj.ShowTo(client);
                        else
                        {
                            bool skip = false;

                            if (obj is Mundane)
                            {

                                if (obj is Mundane mundane)
                                {
                                    var template = mundane.Template;


                                    //hide user if they are not a monk.
                                    if (template.ViewingQualifer.HasFlag(ViewQualifer.Monks))
                                    {
                                        if (client.ClassID != 5)
                                        {
                                            skip = true;
                                        }
                                    }

                                    //hide user if they are not a warrior.
                                    if (template.ViewingQualifer.HasFlag(ViewQualifer.Warriors))
                                    {
                                        if (client.ClassID != 1)
                                        {
                                            skip = true;
                                        }
                                    }

                                    //hide user if they are not a rogue.
                                    if (template.ViewingQualifer.HasFlag(ViewQualifer.Rogues))
                                    {
                                        if (client.ClassID != 2)
                                        {
                                            skip = true;
                                        }
                                    }

                                    //hide user if they are not a wizard.
                                    if (template.ViewingQualifer.HasFlag(ViewQualifer.Wizards))
                                    {
                                        if (client.ClassID != 3)
                                        {
                                            skip = true;
                                        }
                                    }

                                    //hide user if they are not a priest.
                                    if (template.ViewingQualifer.HasFlag(ViewQualifer.Priests))
                                    {
                                        if (client.ClassID != 4)
                                        {
                                            skip = true;
                                        }
                                    }


                                    //TODO add more classes.

                                }
                            }
                            else
                            {
                                if (obj is Money)
                                {
                                    var gold_setting = client.GameSettings.Find(i => i.EnabledSettingStr.Contains("AUTO LOOT GOLD"));

                                    if (gold_setting != null)
                                    {
                                        if (gold_setting.Enabled)
                                        {
                                            (obj as Money).GiveTo((obj as Money).Amount, client);
                                            skip = true;
                                        }
                                    }
                                }
                            }

                            if (!skip)
                                payload.Add(obj);
                        }
                    }
                }
            }
        }

        public static void RemoveObjects(Aisling client, Sprite[] ObjectsToRemove)
        {
            foreach (var obj in ObjectsToRemove)
            {
                if (obj.Serial == client.Serial)
                    continue;

                if (client.View.Contains(obj))
                {
                    if (client.View.Remove(obj))
                    {
                        if (obj is Monster)
                        {
                            (obj as Monster).Script?.OnLeave(client.Client);
                        }
                        obj.HideFrom(client);
                    }
                }
            }
        }
    }
}


