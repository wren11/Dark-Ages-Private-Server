using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Types;
using Newtonsoft.Json;
using System;

namespace Darkages
{
    public class PortalSession
    {
        public PortalSession()
        {
            IsMapOpen = false;
        }

        public bool IsMapOpen { get; set; }
        public int FieldNumber { get; set; }
        public DateTime DateOpened { get; set; }

        [JsonIgnore]
        public WorldMapTemplate Template
            => ServerContext.GlobalWorldMapTemplateCache[FieldNumber];

        public void ShowFieldMap(GameClient client)
        {
            client.Aisling.PortalSession
                = new PortalSession
                {
                    FieldNumber = 1,
                    IsMapOpen = false,
                    DateOpened = DateTime.UtcNow
                };

            client.Send(new ServerFormat2E(client.Aisling));
            client.Aisling.PortalSession.IsMapOpen = true;
        }

        public void TransitionToMap(GameClient client,
            short X = -1, short Y = -1, int DestinationMap = 0)
        {
            if (DestinationMap == 0)
            {
                client.LeaveArea(true, true);

                DestinationMap = ServerContext.Config.TransitionZone;
                var targetMap = ServerContext.GlobalMapCache[DestinationMap];
                client.Aisling.X = X >= 0 ? X : ServerContext.Config.TransitionPointX;
                client.Aisling.Y = Y >= 0 ? Y : ServerContext.Config.TransitionPointY;
                client.Aisling.CurrentMapId = DestinationMap;
                client.Refresh();
                ShowFieldMap(client);
                return;
            }

            if (ServerContext.GlobalMapCache.ContainsKey(DestinationMap))
            {
                var targetMap = ServerContext.GlobalMapCache[DestinationMap];

                if (client.Aisling.AreaID != DestinationMap)
                {
                    client.LeaveArea(true, false);
                    client.Aisling.X = X >= 0 ? X : ServerContext.Config.TransitionPointX;
                    client.Aisling.Y = Y >= 0 ? Y : ServerContext.Config.TransitionPointY;
                    client.Aisling.CurrentMapId = DestinationMap;

                    client.EnterArea();
                    client.Refresh();
                }
            }
        }
    }
}