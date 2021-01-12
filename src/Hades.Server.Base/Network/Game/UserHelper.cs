using Darkages.Types;
using System;

namespace Darkages.Network.Game
{
    internal class UserHelper
    {
        private GameServer gameServer;
        private Mundane mundane;

        public UserHelper(GameServer gameServer, Mundane mundane)
        {
            this.gameServer = gameServer;
            this.mundane = mundane;
        }

        internal void OnResponse(GameServer gameServer, GameClient client, ushort step, string args)
        {

        }

        internal void OnClick(GameServer gameServer, GameClient client)
        {

        }
    }
}