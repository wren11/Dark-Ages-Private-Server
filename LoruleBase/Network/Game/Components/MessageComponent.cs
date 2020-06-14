#region

using System;
using System.Linq;

#endregion

namespace Darkages.Network.Game.Components
{
    public class MessageComponent : GameServerComponent
    {
        public MessageComponent(GameServer server)
            : base(server)
        {
            Timer = new GameServerTimer(
                TimeSpan.FromSeconds(ServerContextBase.Config.MessageClearInterval));
        }

        public GameServerTimer Timer { get; set; }

        public override void Update(TimeSpan elapsedTime)
        {
            Timer.Update(elapsedTime);

            if (Timer.Elapsed)
            {
                Timer.Reset();

                lock (ServerContext.SyncLock)
                {
                    foreach (var client in Server.Clients.Where(Predicate).Where(Selector))
                    {
                        client.SendMessage(0x01, "\0");
                    }
                }
            }
        }

        private static bool Predicate(GameClient client)
        {
            return client?.Aisling != null;
        }

        private static bool Selector(GameClient client)
        {
            return (DateTime.UtcNow - client.LastMessageSent).TotalSeconds > 5;
        }
    }
}