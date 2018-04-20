using System;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;

namespace Darkages.Storage.locales.Scripts.Global
{
    [Script("Grim Reaper", "Dean")]
    public class GrimReaper : GlobalScript
    {
        private readonly GameClient Client;
        private readonly GameServerTimer GrimTimer;

        public GrimReaper(GameClient client) : base(client)
        {
            GrimTimer = new GameServerTimer(TimeSpan.FromMilliseconds(ServerContext.Config.HealoutTolerance));
            Client = client;
        }

        public override void OnDeath(GameClient client, TimeSpan Elapsed)
        {
            if (client == null || client.Aisling == null)
                return;

            if (!client.Aisling.Dead)
                lock (client.Aisling)
                {
                    if (client.Aisling.CurrentHp > 0)
                    {
                        client.SendStats(StatusFlags.All);
                        return;
                    }


                    client.Aisling.RemoveAllBuffs();
                    client.Aisling.RemoveAllDebuffs();

                    if (client.Aisling.Map.Flags.HasFlag(MapFlags.PlayerKill))
                    {
                        client.Aisling.LastMapId = client.Aisling.CurrentMapId;
                        client.Aisling.LastPosition = client.Aisling.Position;

                        client.CloseDialog();
                        client.Aisling.Flags = AislingFlags.Dead;
                        client.HpRegenTimer.Disabled = true;
                        client.MpRegenTimer.Disabled = true;

                        Client.LeaveArea(true, false);
                        client.EnterArea();

                        var target = client.Aisling.Target;

                        if (target != null)
                        {
                            if (target is Aisling)
                                client.SendMessage(Scope.NearbyAislings, 0x02,
                                    client.Aisling.Username + " has been killed by " + (target as Aisling).Username);
                        }
                        else
                        {
                            client.SendMessage(Scope.NearbyAislings, 0x02,
                                client.Aisling.Username + " has been killed.");
                        }

                        return;
                    }


                    if (client.Aisling.Path != Class.Peasant)
                    {
                        client.Aisling.LastMapId = client.Aisling.CurrentMapId;
                        client.Aisling.LastPosition = client.Aisling.Position;

                        client.CloseDialog();
                        client.Aisling.Flags = AislingFlags.Dead;
                        client.HpRegenTimer.Disabled = true;
                        client.MpRegenTimer.Disabled = true;

                        Client.LeaveArea(true, false);
                        client.EnterArea();

                        SendToHell(client);
                    }
                    else
                    {
                        client.SendAnimation(78, client.Aisling, client.Aisling);
                        client.SendMessage(0x02, "You can't die if you have no soul.");
                        client.Aisling.Recover();
                    }

                    client.Send(new ServerFormat08(client.Aisling,
                        StatusFlags.All));
                }
        }

        private static void SendToHell(GameClient client)
        {
            if (!ServerContext.GlobalMapCache.ContainsKey(ServerContext.Config.DeathMap))
                return;

            client.Aisling.Remains.Owner = client.Aisling;

            if (client.Aisling.Inventory.Length > 0 || client.Aisling.EquipmentManager.Length > 0)
            {
                client.Aisling.Remains.ReepItems();
            }

            client.LeaveArea(true, true);
            client.Aisling.X = 21;
            client.Aisling.Y = 21;
            client.Aisling.Direction = 0;
            client.Aisling.CurrentMapId = ServerContext.Config.DeathMap;
            client.Aisling.AreaID = ServerContext.Config.DeathMap;
            client.EnterArea();
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (Client != null && Client.Aisling.LoggedIn)
                if (Client.Aisling.CurrentHp <= 0)
                {
                    Client.Aisling.CurrentHp = 0;

                    GrimTimer.Update(elapsedTime);

                    if (GrimTimer.Elapsed)
                    {
                        OnDeath(Client, elapsedTime);
                        GrimTimer.Reset();
                    }
                }
        }
    }
}