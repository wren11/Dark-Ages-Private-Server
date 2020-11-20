#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script("Class Chooser")]
    public class ClassChooser : MundaneScript
    {
        public ClassChooser(GameServer server, Mundane mundane)
            : base(server, mundane)
        {
        }

        public override void OnClick(GameServer server, GameClient client)
        {
            if (client.Aisling.Path == Class.Peasant)
            {
                var options = new List<OptionsDataItem>
                {
                    new OptionsDataItem(0x06, "I'm ready to choose a Path,"),
                    new OptionsDataItem(0x07, "I'm not ready.")
                };
                client.SendOptionsDialog(Mundane,
                    "Hm? You look weak. you are a peasant. You can't survive this world without a set of skills and discipline. You must make a choice. Now is the time.",
                    options.ToArray());
            }
            else
            {
                client.SendOptionsDialog(Mundane, "You have already chosen your path.");
            }
        }

        public override void OnGossip(GameServer server, GameClient client, string message)
        {
        }

        public override void OnResponse(GameServer server, GameClient client, ushort responseID, string args)
        {
            if (responseID < 0x0001 ||
                responseID > 0x0005)
            {
                if (responseID == 6)
                {
                    var options = new List<OptionsDataItem>
                    {
                        new OptionsDataItem(0x01, "Warrior"),
                        new OptionsDataItem(0x02, "Rogue"),
                        new OptionsDataItem(0x03, "Wizard"),
                        new OptionsDataItem(0x04, "Priest"),
                        new OptionsDataItem(0x05, "Monk")
                    };

                    client.SendOptionsDialog(Mundane, "What do you seek?", options.ToArray());
                }

                if (responseID == 7)
                {
                }
            }
            else
            {
                client.Aisling.Path = (Class)responseID;

                client.SendOptionsDialog(Mundane, $"Congratulations! You are now a {Convert.ToString(client.Aisling.Path)}");

                client.Aisling.Stage = ClassStage.Master;
                client.Aisling.ExpLevel = 1;
                client.Aisling.StatPoints = 98 * 2;

                if (client.Aisling.Path == Class.Priest)
                {
                    Spell.GiveTo(client.Aisling, "armachd", 1);
                   
                    Spell.GiveTo(client.Aisling, "beag ioc", 1);
                    Spell.GiveTo(client.Aisling, "ao beag cradh", 1);

                    Spell.GiveTo(client.Aisling, "beag cradh", 1);
                  
                    Spell.GiveTo(client.Aisling, "deo saighead", 1);
              
                    Spell.GiveTo(client.Aisling, "pramh", 1);

                }

                if (client.Aisling.Path == Class.Wizard)
                {
                    
                    Spell.GiveTo(client.Aisling, "beag puinsein", 1);
                    
                 
                    Spell.GiveTo(client.Aisling, "pramh", 1);
                
                    Spell.GiveTo(client.Aisling, "sal", 1);
                    Spell.GiveTo(client.Aisling, "srad", 1);
                    Spell.GiveTo(client.Aisling, "athar", 1);

                   
               
                    Spell.GiveTo(client.Aisling, "fas nadur", 1);
                    
                 
                    var item = Item.Create(client.Aisling, ServerContext.GlobalItemTemplateCache["Magus Kronos"]);
                    

                    {
                        item.GiveTo(client.Aisling);
                       
                    }
                    var itemMJR = Item.Create(client.Aisling, ServerContext.GlobalItemTemplateCache["Magic Jade Ring"]);
                    {
                        itemMJR.GiveTo(client.Aisling);
                    }
                    var item3 = Item.Create(client.Aisling, ServerContext.GlobalItemTemplateCache["Used Boots"]);
                    {
                        item3.GiveTo(client.Aisling);
                    }
                }

                if (client.Aisling.Path == Class.Warrior)
                {
                    Skill.GiveTo(client.Aisling, "Wind Blade", 1);
                    Skill.GiveTo(client.Aisling, "Charge", 1);
                    Skill.GiveTo(client.Aisling, "Clobber", 1);
                    Skill.GiveTo(client.Aisling, "Assault", 1);
                    Skill.GiveTo(client.Aisling, "Wallop", 1);
                    Skill.GiveTo(client.Aisling, "Two-Handed Attack", 1);
                    Skill.GiveTo(client.Aisling, "Titan's Cleave", 1);
                    Skill.GiveTo(client.Aisling, "Rush", 1);
                    Skill.GiveTo(client.Aisling, "Rescue", 1);
                    Skill.GiveTo(client.Aisling, "beag suain ia gar", 1);
                    Skill.GiveTo(client.Aisling, "beag suain", 1);
                  
                    var item = Item.Create(client.Aisling, ServerContext.GlobalItemTemplateCache["Wooden Shield 2"]);
                    {
                        item.GiveTo(client.Aisling);
                    }
                    var item3 = Item.Create(client.Aisling, ServerContext.GlobalItemTemplateCache["Used Boots"]);
                    {
                        item3.GiveTo(client.Aisling);
                    }
                    var MaleChest = Item.Create(client.Aisling, ServerContext.GlobalItemTemplateCache["Torn Leather Tunic"]);
                    {
                        MaleChest.GiveTo(client.Aisling);
                    }
                    var Sword = Item.Create(client.Aisling, ServerContext.GlobalItemTemplateCache["Eppe"]);
                    {
                        Sword.GiveTo(client.Aisling);
                    }
                }

                if (client.Aisling.Path == Class.Monk)
                {
                    Skill.GiveTo(client.Aisling, "Wolf Fang Fist", 1);
                    Skill.GiveTo(client.Aisling, "Claw Fist", 1);
                    Skill.GiveTo(client.Aisling, "Krane Kick", 1);
                    Skill.GiveTo(client.Aisling, "Claw Fist", 1);
                    Skill.GiveTo(client.Aisling, "Hurricane Kick", 1);
                    Skill.GiveTo(client.Aisling, "Kelberoth Strike", 1);

                    Spell.GiveTo(client.Aisling, "beag ioc fein", 1);
                    Spell.GiveTo(client.Aisling, "dion", 1);
                    Spell.GiveTo(client.Aisling, "armachd", 1);
                }

                if (client.Aisling.Path == Class.Rogue)
                {
                    Skill.GiveTo(client.Aisling, "Unstuck", 1);
                    Skill.GiveTo(client.Aisling, "Locate Player", 1);
                    Skill.GiveTo(client.Aisling, "Locate Monster", 1);
                    Skill.GiveTo(client.Aisling, "Sneak", 1);
                    Skill.GiveTo(client.Aisling, "Rescue", 1);
                    Skill.GiveTo(client.Aisling, "Stab", 1);
                    Skill.GiveTo(client.Aisling, "Inspect Item", 1);

                    Spell.GiveTo(client.Aisling, "beag ioc fein", 1);
                    Spell.GiveTo(client.Aisling, "Poison Trap", 1);
                    Spell.GiveTo(client.Aisling, "Needle Trap", 1);
                    Spell.GiveTo(client.Aisling, "Stiletto Trap", 1);

                    var item = Item.Create(client.Aisling, ServerContext.GlobalItemTemplateCache["Snow Secret"]);
                    {
                        item.GiveTo(client.Aisling);
                    }
                }

                client.CloseDialog();

                client.Aisling.LegendBook.AddLegend(new Legend.LegendItem
                {
                    Category = "Class",
                    Color = (byte)LegendColor.Blue,
                    Icon = (byte)LegendIcon.Victory,
                    Value = $"Devoted to the path of {Convert.ToString(client.Aisling.Path)} "
                });

                client.Aisling.LegendBook.AddLegend(new Legend.LegendItem
                {
                    Category = "Alpha Aisling",
                    Color = (byte)LegendColor.Yellow,
                    Icon = (byte)LegendIcon.Heart,
                    Value = $"Alpha Aisling - Endured the harsh winter of the beginning"
                });

                client.Aisling.GoHome();
                client.SendStats(StatusFlags.All);

                Task.Delay(350).ContinueWith(ct => { client.Aisling.Animate(5); });
            }
        }

        public override void TargetAcquired(Sprite Target)
        {
        }
    }
}