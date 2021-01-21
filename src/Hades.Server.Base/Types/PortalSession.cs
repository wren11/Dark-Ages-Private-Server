#region

using System;
using System.Linq;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Templates;
using Darkages.Types;


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
            => ServerContext.GlobalWorldMapTemplateCache[FieldNumber];

        public void ShowFieldMap(GameClient client)
        {
            if (client.MapOpen)
                return;

            if (ServerContext.GlobalWorldMapTemplateCache.ContainsKey(client.Aisling.World))
            {
                var portal = ServerContext.GlobalWorldMapTemplateCache[client.Aisling.World];

                if (portal.Portals.Any(ports => !ServerContext.GlobalMapCache.ContainsKey(ports.Destination.AreaId)))
                {
                    ServerContext.Logger("No Valid Configured World Map.");
                    return;
                }
            }

            client.Send(new ServerFormat2E(client.Aisling));


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
                client.Aisling.EnterAbyss();
                ShowFieldMap(client);
            }
            else
            {
                if (!ServerContext.GlobalMapCache.ContainsKey(destinationMap))
                    return;

                client.Aisling.XPos = x >= 0 ? x : ServerContext.Config.TransitionPointX;
                client.Aisling.YPos = y >= 0 ? y : ServerContext.Config.TransitionPointY;

                client.Aisling.CurrentMapId = destinationMap;
                client.LeaveArea(true, true);
                client.EnterArea();
            }

            client.Aisling.PortalSession = null;
        }
    }
}