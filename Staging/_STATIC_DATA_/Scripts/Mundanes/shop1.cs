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

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script("shop1", "Dean")]
    public class shop1 : MundaneScript
    {
        public shop1(GameServer server, Mundane mundane) : base(server, mundane)
        {
        }

        public override void OnGossip(GameServer server, GameClient client, string message)
        {
        }

        public override void TargetAcquired(Sprite Target)
        {
        }

        public override void OnClick(GameServer server, GameClient client)
        {
            var opts = new List<OptionsDataItem>();
            opts.Add(new OptionsDataItem(0x0001, "Buy"));
            opts.Add(new OptionsDataItem(0x0002, "Sell"));
            opts.Add(new OptionsDataItem(0x0003, "Repair Items"));

            client.SendOptionsDialog(Mundane, "What you looking for?", opts.ToArray());
        }

        public override void OnResponse(GameServer server, GameClient client, ushort responseID, string args)
        {
            switch (responseID)
            {
                case 0x0001:
                    client.SendItemShopDialog(Mundane, "Have a browse!", 0x0004,
                        ServerContext.GlobalItemTemplateCache.Values.Where(i => i.NpcKey == Mundane.Template.Name)
                            .ToList());
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
                        var offer = Convert.ToString((int)(item.Template.Value / 1.6));

                        var opts2 = new List<OptionsDataItem>();
                        opts2.Add(new OptionsDataItem(0x0019, "Fair enough."));
                        opts2.Add(new OptionsDataItem(0x0020, "decline offer."));

                        client.SendOptionsDialog(Mundane, string.Format(
                            "I will give offer you {0} gold for that {1}, Deal?",
                            offer, item.Template.Name), item.Template.Name, opts2.ToArray());
                    }
                    break;

                case 0x0019:
                    {
                        var v = args;
                        var item = client.Aisling.Inventory.Get(i => i != null && i.Template.Name == v)
                            .FirstOrDefault();

                        if (item == null)
                            return;

                        var offer = Convert.ToString((int)(item.Template.Value / 1.6));

                        if (Convert.ToInt32(offer) <= 0)
                            return;

                        if (Convert.ToInt32(offer) > item.Template.Value)
                            return;

                        if (client.Aisling.GoldPoints + Convert.ToInt32(offer) <= ServerContext.Config.MaxCarryGold)
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

                    //TODO: make this calculate proper repair values.
                    var repair_sum = client.Aisling.Inventory.Items.Where(i => i.Value != null
                                                                               && i.Value.Template.Flags.HasFlag(
                                                                                   ItemFlags.Repairable)).Sum(i =>
                        i.Value.Template.Value / 4);

                    var opts = new List<OptionsDataItem>();
                    opts.Add(new OptionsDataItem(0x0014, "Fair enough."));
                    opts.Add(new OptionsDataItem(0x0015, "Fuck off!"));
                    client.SendOptionsDialog(Mundane,
                        "It will cost " + repair_sum + " Gold to repair everything. Do you Agree?",
                        repair_sum.ToString(), opts.ToArray());

                    break;

                case 0x0014:
                    {
                        var gear = client.Aisling.EquipmentManager.Equipment.Where(i => i.Value != null).Select(i => i.Value.Item);


                        foreach (var item in client.Aisling.Inventory.Items
                            .Where(i => i.Value != null).Select(i => i.Value)
                            .Concat(gear).Where(i => i != null && i.Template.Flags.HasFlag(ItemFlags.Repairable)))
                        {
                            item.Durability = item.Template.MaxDurability;                            
                            client.Aisling.Inventory.UpdateSlot(client, item);
                        }

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
                                //Create Item:
                                var item = Item.Create(client.Aisling, template);

                                if (item.GiveTo(client.Aisling, true))
                                {
                                    client.Aisling.GoldPoints -= (int)template.Value;

                                    if (client.Aisling.GoldPoints < 0)
                                        client.Aisling.GoldPoints = 0;

                                    client.SendStats(StatusFlags.All);
                                    client.SendOptionsDialog(Mundane, string.Format("You have a brand new {0}", args));
                                }
                                else
                                {
                                    client.SendMessage(0x02, "Yeah right, You can't even physically hold it.");
                                    return;
                                }
                            }
                            else
                            {
                                if (ServerContext.GlobalSpellTemplateCache.ContainsKey("beag cradh"))
                                {
                                    var script = ScriptManager.Load<SpellScript>("beag cradh", Spell.Create(1, ServerContext.GlobalSpellTemplateCache["beag cradh"]));
                                    {
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
    }
}
