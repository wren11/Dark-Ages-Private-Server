#region

using System;
using System.Collections.Generic;
using System.Linq;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;

#endregion

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

        public override void OnResponse(GameServer server, GameClient client, ushort responseID, string args)
        {
            try
            {
                if (responseID <= 0 || !client.Aisling.LoggedIn || client.Aisling.Dead ||
                    client.Aisling.BankManager == null)
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
                                $"{Mundane.Template.Name} is finished with you.");

                            client.CloseDialog();
                            return;
                        }

                        var item = client.Aisling.Inventory.Get(i => i != null
                                                                     && i.Slot == Convert.ToInt32(args))
                            .FirstOrDefault();

                        if (item != null)
                        {
                            var cost = (int) (item.Template.Value > 1000 ? item.Template.Value / 30 : 500);

                            if (client.Aisling.GoldPoints >= cost)
                            {
                                Mundane.Show(Scope.NearbyAislings, new ServerFormat0D
                                {
                                    Serial = Mundane.Serial,
                                    Text = $"Great, That will be {cost} coins.",
                                    Type = 0x00
                                });

                                client.Aisling.BankManager.Deposit(item);

                                if (item.Template.Flags.HasFlag(ItemFlags.Stackable) && item.Stacks > 0)
                                {
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
                                }
                            }
                            else
                            {
                                Mundane.Show(Scope.NearbyAislings, new ServerFormat0D
                                {
                                    Serial = Mundane.Serial,
                                    Text = $"Help!, {client.Aisling.Username} is trying to rip me off!",
                                    Type = 0x02
                                });
                                client.CloseDialog();
                            }
                        }
                        else
                        {
                            Mundane.Show(Scope.NearbyAislings, new ServerFormat0D
                            {
                                Serial = Mundane.Serial,
                                Text = $"Help!, {client.Aisling.Username} is trying to scam me!",
                                Type = 0x02
                            });
                            client.CloseDialog();
                        }
                    }
                        break;

                    #endregion

                    #region deposit item confirm

                    case 0x0800:
                    {
                        var item = client.Aisling.Inventory.Get(i => i != null
                                                                     && i.Slot == Convert.ToInt32(args))
                            .FirstOrDefault();

                        if (item != null)
                        {
                            var cost = Convert.ToString((int) (item.Template.Value > 1000
                                ? item.Template.Value / 30
                                : 500));

                            var options = new List<OptionsDataItem>();
                            options.Add(new OptionsDataItem(0x0051, "Confirm"));
                            options.Add(new OptionsDataItem(0x0052, "Cancel"));

                            client.SendOptionsDialog(Mundane,
                                $"I can hold that ({item.DisplayName}) But it will cost {cost} gold.", args,
                                options.ToArray());
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
                            Mundane.Show(Scope.NearbyAislings, new ServerFormat0D
                            {
                                Serial = Mundane.Serial,
                                Text = $"{client.Aisling.Username}, Here is your {itemName} back.",
                                Type = 0x00
                            });
                            WithDrawMenu(client);
                        }
                        else
                        {
                            client.CloseDialog();
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
                            $"{Mundane.Template.Name} is finished with you.");

                        client.CloseDialog();
                    }
                        break;
                }
            }
            catch (Exception)
            {
            }
        }

        public override void TargetAcquired(Sprite Target)
        {
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
                client.Send(new ServerFormat2F(Mundane, "Hello, What can i do you for?",
                    new BankingData(0x08, client.Aisling.Inventory.BankList)));
            else
                OnClick(Server, client);
        }

        private void WithDrawMenu(GameClient client)
        {
            if (client.Aisling.BankManager.Items.Count > 0)
                client.Send(new ServerFormat2F(Mundane, "Hello, What can i do you for?",
                    new WithdrawBankData(0x0A, client.Aisling.BankManager)));
            else
                OnClick(Server, client);
        }
    }
}