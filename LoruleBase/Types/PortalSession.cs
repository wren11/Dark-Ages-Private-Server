using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Types;
using Newtonsoft.Json;
using System;
using System.Threading;

namespace Darkages
{
    public class PortalSession
    {
        public PortalSession()
        {
            IsMapOpen = false;
        }

        public bool IsMapOpen { get; set; }
        public int FieldNumber { get; set; } = 1;
        public DateTime DateOpened { get; set; }

        [JsonIgnore]
        public WorldMapTemplate Template
            => ServerContextBase.GlobalWorldMapTemplateCache[FieldNumber];

        public void ShowFieldMap(GameClient client)
        {
            lock (ServerContext.syncLock)
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

        public void TransitionToMap(GameClient client, short X = -1, short Y = -1, int DestinationMap = 0)
        {
            client.LastWarp = DateTime.UtcNow.AddMilliseconds(100);

            if (DestinationMap == 0)
            {
                client.LeaveArea(true, true);

                DestinationMap = ServerContextBase.GlobalConfig.TransitionZone;

                client.Aisling.XPos = X >= 0 ? X : ServerContextBase.GlobalConfig.TransitionPointX;
                client.Aisling.YPos = Y >= 0 ? Y : ServerContextBase.GlobalConfig.TransitionPointY;

                client.Aisling.CurrentMapId = DestinationMap;
                client.LeaveArea(true, true);
                client.EnterArea();

                ShowFieldMap(client);
            }
            else
            {
                if (!ServerContextBase.GlobalMapCache.ContainsKey(DestinationMap))
                    return;

                client.Aisling.XPos = X >= 0 ? X : ServerContextBase.GlobalConfig.TransitionPointX;
                client.Aisling.YPos = Y >= 0 ? Y : ServerContextBase.GlobalConfig.TransitionPointY;

                client.Aisling.CurrentMapId = DestinationMap;
                client.LeaveArea(true, true); 
                client.EnterArea();
            }

            client.Aisling.PortalSession = null;
        }
    }
}