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
            lock (Server.Clients)
            {
                var connected_users = Server.Clients.Where(i =>
                    i != null &&
                    i.Aisling != null &&
                    i.Aisling.Map != null &&
                    i.Aisling.Map.Ready).Select(i => i.Aisling).ToArray();
              
                for (int i = 0; i < connected_users.Length; i++)
                {
                    var payload = new List<Sprite>();
                    var client  = connected_users[i];

                    if (client.LoggedIn && client.Map.Ready)
                    {
                        var objects = GetObjects(client.Map, selector => selector != null, Get.All);

                        var objectsInView = objects.Where(s => s.WithinRangeOf(client));
                        var objectsNotInView = objects.Where(s => !s.WithinRangeOf(client));

                        var ObjectsToRemove = objectsNotInView.Except(objectsInView).ToArray();
                        var ObjectsToAdd = objectsInView.Except(objectsNotInView).ToArray();

                        RemoveObjects(
                            client,
                            ObjectsToRemove);

                        AddObjects(payload,
                            client,
                            ObjectsToAdd);

                        if (payload.Count > 0)
                        {
                            client.Show(Scope.Self, new ServerFormat07(payload.ToArray()));
                        }
                    }
                }
            }
        }

        private static void AddObjects(List<Sprite> payload, Aisling client, Sprite[] ObjectsToAdd)
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
                            payload.Add(obj);
                        }
                    }
                }
            }
        }

        private static void RemoveObjects(Aisling client, Sprite[] ObjectsToRemove)
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


