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
    [Script("shop1", "Dean")]
    public class shop1 : MundaneScript
    {
        public shop1(GameServer server, Mundane mundane) : base(server, mundane)
        {
        }

        public override void OnClick(GameServer server, GameClient client)
        {
            var opts = new List<OptionsDataItem>
            {
                new OptionsDataItem(0x0001, "Buy"),
                new OptionsDataItem(0x0002, "Sell"),
                new OptionsDataItem(0x0003, "Repair Items")
            };

            client.SendOptionsDialog(Mundane, "What you looking for?", opts.ToArray());
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

            var opts = new List<OptionsDataItem>();
            switch (responseID)
            {
                case 0x0001:
                    client.SendItemShopDialog(Mundane, "Have a browse!", 0x0004,
                        ServerContext.GlobalItemTemplateCache.Values.Where(i => i.NpcKey == Mundane.Template.Name)
                            .ToList().Concat(defaultbag.Where(n => n != null)));
                    break;

                case 0x0002:
                    client.SendItemSellDialog(Mundane, "What do you want to pawn?", 0x0005,
                        client.Aisling.Inventory.Items.Values.Where(i => i != null && i.Template != null)
                            .Select(i => i.Slot).ToList());

                    break;

                case 0x0500:
                {
                    var item = client.Aisling.Inventory.Get(i => i != null && i.Slot == Convert.ToInt32(args))
                        .FirstOrDefault();
                    var offer = Convert.ToString((int) (item.Template.Value / 1.6));

                    var opts2 = new List<OptionsDataItem>
                    {
                        new OptionsDataItem(0x0019, "Fair enough."), new OptionsDataItem(0x0020, "decline offer.")
                    };

                    client.SendOptionsDialog(Mundane,
                        $"I will give offer you {offer} gold for that {item.Template.Name}, Deal?", item.Template.Name,
                        opts2.ToArray());
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

                        client.SendOptionsDialog(Mundane, "Sucker.");
                    }
                }
                    break;

                #region Buy

                case 0x0003:

                    var repair_sum = client.Aisling.Inventory.Items.Where(i => i.Value != null
                                                                               && i.Value.Template.Flags.HasFlag(
                                                                                   ItemFlags.Repairable)).Sum(i =>
                        i.Value.Template.Value / 4);

                    opts.Add(new OptionsDataItem(0x0014, "Fair enough."));
                    opts.Add(new OptionsDataItem(0x0015, "Fuck off!"));
                    client.SendOptionsDialog(Mundane,
                        "It will cost " + repair_sum + " Gold to repair everything. Do you Agree?",
                        repair_sum.ToString(), opts.ToArray());

                    break;

                case 0x0014:
                {
                    var gear = client.Aisling.EquipmentManager.Equipment.Where(i => i.Value != null)
                        .Select(i => i.Value.Item);

                    client.RepairEquipment(gear);

                    client.SendOptionsDialog(Mundane, "All done, now go away.");
                }
                    break;

                case 0x0015:
                    client.SendOptionsDialog(Mundane, "well then. i will see you later.");
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
                                client.SendMessage(0x02, "Yeah right, You can't even physically hold it.");
                            }
                        }
                        else
                        {
                            if (ServerContext.GlobalSpellTemplateCache.ContainsKey("beag cradh"))
                            {
                                var scripts = ScriptManager.Load<SpellScript>("beag cradh",
                                    Spell.Create(1, ServerContext.GlobalSpellTemplateCache["beag cradh"]));
                                {
                                    foreach (var script in scripts.Values)
                                        script.OnUse(Mundane, client.Aisling);
                                }
                                client.SendOptionsDialog(Mundane, "You trying to rip me off?! go away.");
                            }
                        }
                }
                    break;

                #endregion Buy
            }
        }

        public override void TargetAcquired(Sprite Target)
        {
        }
    }
}