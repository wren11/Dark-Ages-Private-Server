///************************************************************************
//Project Lorule: A Dark Ages Server (http://darkages.creatorlink.net/index/)
//Copyright(C) 2018 TrippyInc Pty Ltd
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
//*************************************************************************/
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Storage.locales.debuffs;
using Darkages.Types;
using System;

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

                    if (client.Aisling.Map.Flags.HasFlag(MapFlags.PlayerKill))
                    {
                        CastDeath(client);

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
                        if (!client.Aisling.HasDebuff("skulled"))
                        {
                            var debuff = new debuff_reeping();
                            {
                                debuff.OnApplied(client.Aisling, debuff);
                            }
                        }
                    }
                    else
                    {
                        if (client.Aisling.AreaID != 85)
                        {
                            client.SendAnimation(78, client.Aisling, client.Aisling);
                            client.SendMessage(0x02, "You can't die if you have no soul.");
                            client.Aisling.Recover();
                        }
                        else
                        {
                            for (int i = 0; i < 2; i++)
                                client.Aisling.RemoveBuffsAndDebuffs();

                            client.TransitionToMap(1006, new Position(2, 4));
                            client.Aisling.TutorialCompleted = true;
                            client.SendMessage(0x02, "You awake from a bad dream... or was it??");
                            client.SendAnimation(94, client.Aisling, client.Aisling);
                            client.Aisling.Recover();
                        }
                    }

                    client.Send(new ServerFormat08(client.Aisling,
                        StatusFlags.All));
                }
        }

        public static void CastDeath(GameClient client)
        {
            client.Aisling.LastMapId = client.Aisling.CurrentMapId;
            client.Aisling.LastPosition = client.Aisling.Position;

            client.CloseDialog();
            client.Aisling.Flags = AislingFlags.Dead;
            client.HpRegenTimer.Disabled = true;
            client.MpRegenTimer.Disabled = true;

            client.LeaveArea(true, false);
            client.EnterArea();
        }

        public static void SendToHell(GameClient client)
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
            client.EnterArea();

            client.Send(new ServerFormat08(client.Aisling, StatusFlags.All));
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
