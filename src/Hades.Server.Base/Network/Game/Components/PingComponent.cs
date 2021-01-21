#region

using System;
using System.Linq;
using Darkages.Network.ServerFormats;
using Darkages.Server.Network.WS;

#endregion

namespace Darkages.Network.Game.Components
{
    public class PingComponent : GameServerComponent
    {
        public PingComponent(GameServer server)
            : base(server)
        {
            Timer = new GameServerTimer(
                TimeSpan.FromSeconds(ServerContext.Config.PingInterval));
        }

        public GameServerTimer Timer { get; set; }

        protected internal override void Update(TimeSpan elapsedTime)
        {
            if (Timer.Update(elapsedTime))
            {
                lock (Server.Clients)
                {
                    foreach (var client in Server.Clients.Where(client => client != null && client.Aisling != null))
                    {
                        client.Send(new ServerFormat3B());
                        client.LastPing = DateTime.UtcNow;
                    }
                }


                ObjectServer.Broadcast($"[System]: Server Up Time: {DateTime.UtcNow - ServerContext.TimeServerStarted}");
                ObjectServer.Broadcast($"[System]: Connected Players: {GetObjects<Aisling>(null, n => n.LoggedIn).Count()}");
                ObjectServer.Broadcast($"[System]: Game Objects: {GetObjects(null, n => n != null, Get.All).Count()}");
                ObjectServer.Broadcast($"[System]: Active Maps: {string.Join(", ", GetObjects<Aisling>(null, n => n != null && (n.Map != null && n.Map.ActiveMap != null)).Select(n => n?.Map.ActiveMap).GroupBy(n => n).Select(o => o.Key).ToArray())}");
            }
        }
    }
}