using System;

namespace Darkages.Network.Game.Components
{
    public class MessageComponent : GameServerComponent
    {
        public MessageComponent(GameServer server)
            : base(server)
        {
            Timer = new GameServerTimer(
                TimeSpan.FromSeconds(ServerContext.Config.MessageClearInterval));
        }

        public GameServerTimer Timer { get; set; }

        public override void Update(TimeSpan elapsedTime)
        {
            Timer.Update(elapsedTime);

            if (Timer.Elapsed)
            {
                Timer.Reset();

                foreach (var client in Server.Clients)
                    if (client != null &&
                        client.Aisling != null)
                        if ((DateTime.UtcNow - client.LastMessageSent).TotalSeconds > 5)
                            client.SendMessage(0x01, "\0");
            }
        }
    }
}