#region

using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Types;
using Newtonsoft.Json;
using System;

#endregion

namespace Darkages
{
    public class PortalSession
    {
        public PortalSession()
        {
            IsMapOpen = false;
        }

        public DateTime DateOpened { get; set; }
        public int FieldNumber { get; set; } = 1;
        public bool IsMapOpen { get; set; }

        [JsonIgnore]
        public WorldMapTemplate Template
            => ServerContextBase.GlobalWorldMapTemplateCache[FieldNumber];

        public void ShowFieldMap(GameClient client)
        {
            lock (ServerContext.SyncLock)
            {
                client.InMapTransition = true;
                client.Send(new ServerFormat2E(client.Aisling));
            }

            client.Aisling.PortalSession
                = new PortalSession
                {
                    FieldNumber = client.Aisling.World,
                    IsMapOpen = true,
                    DateOpened = DateTime.UtcNow
                };
        }

        public void TransitionToMap(GameClient client, short x = -1, short y = -1, int destinationMap = 0)
        {
            client.LastWarp = DateTime.UtcNow.AddMilliseconds(100);

            if (destinationMap == 0)
            {
                client.LeaveArea(true, true);

                destinationMap = ServerContextBase.Config.TransitionZone;

                client.Aisling.XPos = x >= 0 ? x : ServerContextBase.Config.TransitionPointX;
                client.Aisling.YPos = y >= 0 ? y : ServerContextBase.Config.TransitionPointY;

                client.Aisling.CurrentMapId = destinationMap;
                client.LeaveArea(true, true);
                client.EnterArea();

                ShowFieldMap(client);
            }
            else
            {
                if (!ServerContextBase.GlobalMapCache.ContainsKey(destinationMap))
                    return;

                client.Aisling.XPos = x >= 0 ? x : ServerContextBase.Config.TransitionPointX;
                client.Aisling.YPos = y >= 0 ? y : ServerContextBase.Config.TransitionPointY;

                client.Aisling.CurrentMapId = destinationMap;
                client.LeaveArea(true, true);
                client.EnterArea();
            }

            client.Aisling.PortalSession = null;
        }
    }
}