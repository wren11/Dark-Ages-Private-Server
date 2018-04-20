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
using Darkages.Types;
using System.Collections.Generic;

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script("Arena Master")]
    public class ArenaMaster : MundaneScript
    {
        public ArenaMaster(GameServer server, Mundane mundane)
            : base(server, mundane)
        {
        }

        public override void TargetAcquired(Sprite Target)
        {
        }

        public override void OnGossip(GameServer server, GameClient client, string message)
        {
        }

        public override void OnClick(GameServer server, GameClient client)
        {
            var options = new List<OptionsDataItem>();
            options.Add(new OptionsDataItem(0x0001, "North"));
            options.Add(new OptionsDataItem(0x0002, "East"));
            options.Add(new OptionsDataItem(0x0003, "South"));
            options.Add(new OptionsDataItem(0x0004, "West"));
            options.Add(new OptionsDataItem(0x0005, "Middle"));
            options.Add(new OptionsDataItem(0x0006, "Leave Arena"));
            client.SendOptionsDialog(Mundane, "Give the orders.", options.ToArray());
        }

        public override void OnResponse(GameServer server, GameClient client, ushort responseID, string args)
        {
            if (responseID == 0x0006)
                client.SendOptionsDialog(Mundane, "Are you sure you want to leave?",
                    new OptionsDataItem(0x0060, "Leave"),
                    new OptionsDataItem(0x0070, "Continue Fighting"));

            if (responseID == 0x0060)
            {
                client.Aisling.CurrentHp = client.Aisling.MaximumHp;
                client.Aisling.Flags = AislingFlags.Normal;
                client.HpRegenTimer.Disabled = false;
                client.MpRegenTimer.Disabled = false;

                client.LeaveArea(true, false);
                client.EnterArea();
                client.SendStats(StatusFlags.All);

                client.Aisling.PortalSession = new PortalSession { IsMapOpen = false, FieldNumber = 1 };
                client.Aisling.PortalSession.TransitionToMap(client);
                client.CloseDialog();
            }

           
            if (responseID == 0x0001)
            {
                if (client.Aisling.CurrentMapId == 508)
                {
                    client.TransitionToMap(509, new Position(4, 4));
                }
                else
                {
                    client.LeaveArea(true, false);
                    client.Aisling.X = 4;
                    client.Aisling.Y = 4;
                    client.EnterArea();
                    client.CloseDialog();
                }
            }

            if (responseID == 0x0002)
            {
                if (client.Aisling.CurrentMapId == 508)
                {
                    client.TransitionToMap(509, new Position(51, 4));
                }
                else
                {
                    client.LeaveArea(true, false);
                    client.Aisling.X = 51;
                    client.Aisling.Y = 4;
                    client.EnterArea();
                    client.CloseDialog();
                }
            }

            if (responseID == 0x0003)
            {
                if (client.Aisling.CurrentMapId == 508)
                {
                    client.TransitionToMap(509, new Position(51, 51));
                }
                else
                {
                    client.LeaveArea(true, false);
                    client.Aisling.X = 51;
                    client.Aisling.Y = 51;
                    client.EnterArea();
                    client.CloseDialog();
                }
            }

            if (responseID == 0x0004)
            {
                if (client.Aisling.CurrentMapId == 508)
                {
                    client.TransitionToMap(509, new Position(4, 51));
                }
                else
                {
                    client.LeaveArea(true, false);
                    client.Aisling.X = 4;
                    client.Aisling.Y = 51;
                    client.EnterArea();
                    client.CloseDialog();
                }
            }

            if (responseID == 0x0005)
            {
                if (client.Aisling.CurrentMapId == 508)
                {
                    client.TransitionToMap(509, new Position(35, 35));
                }
                else
                {
                    client.LeaveArea(true, false);
                    client.Aisling.X = 35;
                    client.Aisling.Y = 35;
                    client.EnterArea();
                    client.CloseDialog();
                }
            }
        }
    }
}
