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
using System;
using System.Collections.Generic;
using System.Linq;

namespace Darkages.Storage.locales.Scripts.Mundanes.LORULE_CITY.Bankers
{
    [Script("Banker")]
    public class Merchant : MundaneScript
    {
        public Merchant(GameServer server, Mundane mundane) : base(server, mundane)
        {

        }

        public override void OnClick(GameServer server, GameClient client)
        {
            var options = new List<OptionsDataItem>();
            options.Add(new OptionsDataItem(0x0001, "Deposit Item"));
            options.Add(new OptionsDataItem(0x0002, "Withdraw Item"));

            client.SendOptionsDialog(Mundane, "Hello, What can i do you for?", options.ToArray());
        }

        public override void OnGossip(GameServer server, GameClient client, string message)
        {

        }

        void WithDrawMenu(GameClient client)
        {
            if (client.Aisling.BankManager.Items.Count > 0)
            {
                client.Send(new ServerFormat2F(Mundane, "Hello, What can i do you for?",
                    new WithdrawBankData(0x0A, client.Aisling.BankManager)));
            }
            else
            {
                OnClick(Server, client);
            }
        }

        public override void OnResponse(GameServer server, GameClient client, ushort responseID, string args)
        {
            try
            {
                if (responseID <= 0 || !client.Aisling.LoggedIn || client.Aisling.Dead || client.Aisling.BankManager == null)
                {
                    client.SendMessage(0x02, "you should best fuck off.");
                    client.CloseDialog();
                    return;
                }

                if (!client.Aisling.WithinRangeOf(Mundane))
                {
                    client.Server.ClientDisconnected(client);
                    return;
                }

                switch (responseID)
                {
                    #region deposit item
                    case 0x01:
                        {
                            DepositMenu(client);
                        }
                        break;
                    #endregion
                    #region deposit item response
                    case 0x51:
                        {

                            var slot = -1;
                            int.TryParse(args, out slot);

                            if (slot < 0)
                            {
                                client.SendMessage(0x02,
                                    string.Format("{0} is finished with you.",
                                    Mundane.Template.Name));

                                client.CloseDialog();
                                return;
                            }

                            var item = client.Aisling.Inventory.Get(i => i != null
                                && i.Slot == Convert.ToInt32(args)).FirstOrDefault();

                            if (item != null)
                            {
                                var cost = (int)(item.Template.Value > 1000 ? item.Template.Value / 30 : 500);

                                if (client.Aisling.GoldPoints >= cost)
                                {
                                    Mundane.Show(Scope.NearbyAislings, new ServerFormat0D()
                                    {
                                        Serial = Mundane.Serial,
                                        Text = string.Format("Great, That will be {0} coins.", cost),
                                        Type = 0x00
                                    });

                                    client.Aisling.BankManager.Deposit(item);

                                    if (item.Template.Flags.HasFlag(ItemFlags.Stackable) && item.Stacks > 0)
                                    {
                                        //remaining, just take one.
                                        client.Aisling.Inventory.RemoveRange(client, item, 1);
                                        CompleteTrade(client, cost);
                                    }
                                    else if (client.Aisling.EquipmentManager.RemoveFromInventory(item, true))
                                    {
                                        CompleteTrade(client, cost);
                                    }
                                    else
                                    {
                                        client.CloseDialog();
                                        client.SendMessage(0x02, "You could not deposit that.");
                                        return;
                                    }
                                }
                                else
                                {
                                    Mundane.Show(Scope.NearbyAislings, new ServerFormat0D()
                                    {
                                        Serial = Mundane.Serial,
                                        Text = string.Format("Help!, {0} is trying to rip me off!", client.Aisling.Username),
                                        Type = 0x02
                                    });
                                    client.CloseDialog();
                                    return;
                                }
                            }
                            else
                            {
                                Mundane.Show(Scope.NearbyAislings, new ServerFormat0D()
                                {
                                    Serial = Mundane.Serial,
                                    Text = string.Format("Help!, {0} is trying to scam me!", client.Aisling.Username),
                                    Type = 0x02
                                });
                                client.CloseDialog();
                                return;
                            }

                        }
                        break;
                    #endregion
                    #region deposit item confirm
                    case 0x0800:
                        {
                            var item = client.Aisling.Inventory.Get(i => i != null
                                && i.Slot == Convert.ToInt32(args)).FirstOrDefault();

                            if (item != null)
                            {
                                var cost = Convert.ToString((int)(item.Template.Value > 1000 ? item.Template.Value / 30 : 500));


                                var options = new List<OptionsDataItem>();
                                options.Add(new OptionsDataItem(0x0051, "Confirm"));
                                options.Add(new OptionsDataItem(0x0052, "Cancel"));

                                client.SendOptionsDialog(Mundane,
                                    string.Format("I can hold that ({0}) But it will cost {1} gold.",
                                    item.DisplayName, cost), args, options.ToArray());

                            }
                        }
                        break;
                    #endregion
                    #region Withdraw Item
                    case 0x02:
                        {
                            WithDrawMenu(client);
                        }
                        break;
                    #endregion
                    #region Withdraw Item Response
                    case 0x000A:
                        {
                            var itemName = args;

                            if (client.Aisling.BankManager.Withdraw(client, itemName))
                            {
                                Mundane.Show(Scope.NearbyAislings, new ServerFormat0D()
                                {
                                    Serial = Mundane.Serial,
                                    Text = string.Format("{0}, Here is your {1} back.", client.Aisling.Username, itemName),
                                    Type = 0x00
                                });
                                WithDrawMenu(client);

                            }
                            else
                            {
                                client.CloseDialog();
                                return;
                            }

                        }
                        break;
                    #endregion
                    case 0x52:
                        {
                            DepositMenu(client);
                        }
                        break;
                    default:
                        {
                            client.SendMessage(0x02,
                                string.Format("{0} is finished with you.",
                                Mundane.Template.Name));

                            client.CloseDialog();
                        }
                        break;
                }
            }
            catch (Exception)
            {
                //ignore
            }
        }

        private void CompleteTrade(GameClient client, int cost)
        {
            client.Aisling.GoldPoints -= cost;
            client.SendStats(StatusFlags.StructC);
            DepositMenu(client);
        }

        private void DepositMenu(GameClient client)
        {
            if (client.Aisling.Inventory.BankList.Any())
            {
                client.Send(new ServerFormat2F(Mundane, "Hello, What can i do you for?",
                    new BankingData(0x08, client.Aisling.Inventory.BankList)));
            }
            else
            {
                OnClick(Server, client);
            }
        }

        public override void TargetAcquired(Sprite Target)
        {

        }
    }
}
