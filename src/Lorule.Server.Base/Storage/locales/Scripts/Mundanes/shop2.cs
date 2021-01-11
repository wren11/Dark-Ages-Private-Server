#region

using System;
using System.Collections.Generic;
using System.Linq;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script("shop2", "Dean")]
    public class shop2 : MundaneScript
    {
        public shop2(GameServer server, Mundane mundane) : base(server, mundane)
        {
        }

        public override void OnClick(GameServer server, GameClient client)
        {
            TopMenu(client);
        }

        public override void OnGossip(GameServer server, GameClient client, string message)
        {
        }

        public override void OnResponse(GameServer server, GameClient client, ushort responseID, string args)
        {
            var defaultbag = Mundane.Template.DefaultMerchantStock.Select(i =>
                ServerContext.GlobalItemTemplateCache.ContainsKey(i)
                    ? ServerContext.GlobalItemTemplateCache[i]
                    : null);

            switch (responseID)
            {
                case 0x0001:
                    client.SendItemShopDialog(Mundane, "I stock only high-end gear.", 0x0004,
                        ServerContext.GlobalItemTemplateCache.Values.Where(i => i.NpcKey == Mundane.Template.Name)
                            .OrderBy(i => i.LevelRequired).ToList().Concat(defaultbag.Where(n => n != null)));
                    break;

                case 0x0002:
                    client.SendItemSellDialog(Mundane, "What do you want to sell?", 0x0005,
                        client.Aisling.Inventory.Items.Values.Where(i => i != null && i.Template != null)
                            .Select(i => i.Slot).ToList());

                    break;

                case 0x0030:
                {
                    if (client.PendingItemSessions != null)
                    {
                        if (ServerContext.GlobalItemTemplateCache.ContainsKey(client.PendingItemSessions.Name))
                        {
                            var item = client.Aisling.Inventory
                                .Get(i => i != null && i.Template.Name == client.PendingItemSessions.Name)
                                .FirstOrDefault();

                            if (item != null)
                                if (client.Aisling.GiveGold(client.PendingItemSessions.Offer))
                                {
                                    client.Aisling.Inventory.RemoveRange(client, item,
                                        client.PendingItemSessions.Removing);
                                    client.PendingItemSessions = null;
                                    TopMenu(client);

                                    return;
                                }
                        }

                        client.PendingItemSessions = null;
                        TopMenu(client);
                    }
                }
                    break;

                case 0x0000:
                {
                    if (string.IsNullOrEmpty(args))
                        return;

                    int.TryParse(args, out var amount);

                    if (amount > 0 && client.PendingItemSessions != null)
                    {
                        client.PendingItemSessions.Quantity = amount;

                        var item = client.Aisling.Inventory
                            .Get(i => i != null && i.Template.Name == client.PendingItemSessions.Name).FirstOrDefault();

                        if (item != null)
                        {
                            var offer = Convert.ToString((int) (item.Template.Value / 1.6));

                            if (item.Stacks >= amount)
                            {
                                if (client.Aisling.GoldPoints + Convert.ToInt32(offer) <=
                                    ServerContext.Config.MaxCarryGold)
                                {
                                    client.PendingItemSessions.Offer = Convert.ToInt32(offer) * amount;
                                    client.PendingItemSessions.Removing = amount;

                                    var opts2 = new List<OptionsDataItem>
                                    {
                                        new OptionsDataItem(0x0030,
                                            ServerContext.Config.MerchantConfirmMessage),
                                        new OptionsDataItem(0x0020,
                                            ServerContext.Config.MerchantCancelMessage)
                                    };

                                    client.SendOptionsDialog(Mundane, string.Format(
                                            "I will give offer you {0} gold for {1} of those ({2} Gold Each), Deal?",
                                            client.PendingItemSessions.Offer,
                                            amount, client.PendingItemSessions.Offer / amount, item.Template.Name),
                                        opts2.ToArray());
                                }
                            }
                            else
                            {
                                client.PendingItemSessions = null;
                                client.SendOptionsDialog(Mundane,
                                    ServerContext.Config.MerchantStackErrorMessage);
                            }
                        }
                    }
                    else
                    {
                        client.SendOptionsDialog(Mundane, ServerContext.Config.MerchantTradeErrorMessage);
                    }
                }
                    break;

                case 0x0500:
                {
                    var item = client.Aisling.Inventory.Get(i => i != null && i.Slot == Convert.ToInt32(args))
                        .FirstOrDefault();
                    var offer = Convert.ToString((int) (item.Template.Value / 1.6));

                    if (offer == "0")
                    {
                        client.SendOptionsDialog(Mundane, ServerContext.Config.MerchantRefuseTradeMessage);
                        return;
                    }

                    if (item.Stacks > 1 && item.Template.CanStack)
                    {
                        client.PendingItemSessions = new PendingSell
                        {
                            Name = item.Template.Name,
                            Quantity = 0
                        };

                        client.Send(new ServerFormat2F(Mundane,
                            $"How many [{item.Template.Name}] do you want to sell?",
                            new TextInputData()));
                    }
                    else
                    {
                        var opts2 = new List<OptionsDataItem>
                        {
                            new OptionsDataItem(0x0019, ServerContext.Config.MerchantConfirmMessage),
                            new OptionsDataItem(0x0020, ServerContext.Config.MerchantCancelMessage)
                        };

                        client.SendOptionsDialog(Mundane,
                            $"I will give offer you {offer} gold for that {item.Template.Name}, Deal?",
                            item.Template.Name, opts2.ToArray());
                    }
                }
                    break;

                case 0x0019:
                {
                    var v = args;
                    var item = client.Aisling.Inventory.Get(i => i != null && i.Template.Name == v)
                        .FirstOrDefault();

                    if (item == null)
                        return;

                    var offer = Convert.ToString((int) (item.Template.Value / 1.6));

                    if (Convert.ToInt32(offer) <= 0)
                        return;

                    if (Convert.ToInt32(offer) > item.Template.Value)
                        return;

                    if (client.Aisling.GoldPoints + Convert.ToInt32(offer) <=
                        ServerContext.Config.MaxCarryGold)
                    {
                        client.Aisling.GoldPoints += Convert.ToInt32(offer);
                        client.Aisling.EquipmentManager.RemoveFromInventory(item, true);
                        client.SendStats(StatusFlags.StructC);

                        client.SendOptionsDialog(Mundane, ServerContext.Config.MerchantTradeCompletedMessage);
                    }
                }
                    break;

                #region Buy

                case 0x0003:

                    var repair_sum = client.Aisling.Inventory.Items.Where(i => i.Value != null
                                                                               && i.Value.Template.Flags.HasFlag(
                                                                                   ItemFlags.Repairable)).Sum(i =>
                        i.Value.Template.Value / 4);

                    if (repair_sum > 0)
                    {
                        var opts = new List<OptionsDataItem>
                        {
                            new OptionsDataItem(0x0014, ServerContext.Config.MerchantConfirmMessage),
                            new OptionsDataItem(0x0015, ServerContext.Config.MerchantCancelMessage)
                        };
                        client.SendOptionsDialog(Mundane,
                            "It will cost " + repair_sum + " Gold to repair everything. Do you Agree?",
                            repair_sum.ToString(), opts.ToArray());
                    }
                    else
                    {
                        client.SendOptionsDialog(Mundane, "You have nothing that needs repairing.");
                    }

                    break;

                case 0x0014:
                {
                    var gear = client.Aisling.EquipmentManager.Equipment.Where(i => i.Value != null)
                        .Select(i => i.Value.Item);

                    client.RepairEquipment(gear);

                    client.SendOptionsDialog(Mundane, ServerContext.Config.MerchantTradeCompletedMessage);
                }
                    break;

                case 0x0015:
                    client.SendOptionsDialog(Mundane, ServerContext.Config.MerchantCancelMessage);
                    break;

                case 0x0020:
                {
                    client.PendingItemSessions = null;
                    client.SendOptionsDialog(Mundane, ServerContext.Config.MerchantCancelMessage);
                }
                    break;

                case 0x0004:
                {
                    if (string.IsNullOrEmpty(args))
                        return;

                    if (!ServerContext.GlobalItemTemplateCache.ContainsKey(args))
                        return;

                    var template = ServerContext.GlobalItemTemplateCache[args];
                    if (template != null)
                        if (client.Aisling.GoldPoints >= template.Value)
                        {
                            var item = Item.Create(client.Aisling, template);

                            if (item.GiveTo(client.Aisling))
                            {
                                client.Aisling.GoldPoints -= (int) template.Value;

                                if (client.Aisling.GoldPoints < 0)
                                    client.Aisling.GoldPoints = 0;

                                client.SendStats(StatusFlags.All);
                                client.SendOptionsDialog(Mundane, $"You have a brand new {args}");
                            }
                            else
                            {
                                client.SendMessage(0x02,
                                    "You could not buy this item, because you can't physically hold it.");
                            }
                        }
                        else
                        {
                            if (ServerContext.GlobalSpellTemplateCache.ContainsKey("ard cradh"))
                            {
                                var scripts = ScriptManager.Load<SpellScript>("ard cradh",
                                    Spell.Create(1, ServerContext.GlobalSpellTemplateCache["ard cradh"]));

                                foreach (var script in scripts.Values)
                                    script.OnUse(Mundane, client.Aisling);

                                client.SendOptionsDialog(Mundane,
                                    ServerContext.Config.MerchantWarningMessage);
                            }
                        }
                }
                    break;

                #endregion Buy
            }
        }

        public override void TargetAcquired(Sprite target)
        {
        }

        private void TopMenu(GameClient client)
        {
            var opts = new List<OptionsDataItem>
            {
                new OptionsDataItem(0x0001, ServerContext.Config.MerchantBuy),
                new OptionsDataItem(0x0002, ServerContext.Config.MerchantSell),
                new OptionsDataItem(0x0003, "Repair Items")
            };

            client.SendOptionsDialog(Mundane, ServerContext.Config.MerchantBuyMessage, opts.ToArray());
        }
    }
}