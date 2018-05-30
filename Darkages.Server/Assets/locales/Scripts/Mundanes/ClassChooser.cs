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

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script("Class Chooser")]
    public class ClassChooser : MundaneScript
    {
        public ClassChooser(GameServer server, Mundane mundane)
            : base(server, mundane)
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
            if (client.Aisling.ClassID == 0)
            {
                var options = new List<OptionsDataItem>();
                options.Add(new OptionsDataItem(0x06, "I'm ready to choose a Path,"));
                options.Add(new OptionsDataItem(0x07, "I'm not ready."));
                client.SendOptionsDialog(Mundane, "Hm? You look weak. you are a peasant. You can't survive this world without a set of skills and discipline. You must make a choice. Now is the time.", options.ToArray());

            }
            else
            {
                client.SendOptionsDialog(Mundane, "You have already chosen your path.");
            }
        }

        public override void OnResponse(GameServer server, GameClient client, ushort responseID, string args)
        {
            if (responseID < 0x0001 ||
                responseID > 0x0005)
            {

                if (responseID == 6)
                {
                    var options = new List<OptionsDataItem>();
                    options.Add(new OptionsDataItem(0x01, "I devote myself to the Warrior"));
                    options.Add(new OptionsDataItem(0x02, "I devote myself to the Rogue"));
                    options.Add(new OptionsDataItem(0x03, "I devote myself to the Wizard."));
                    options.Add(new OptionsDataItem(0x04, "I devote myself to the priest."));
                    options.Add(new OptionsDataItem(0x05, "I devote myself to the monk."));

                    client.SendOptionsDialog(Mundane, "It is time to make a choice. Peasant. You must begin your journey here, Now.", options.ToArray());
                }
                if (responseID == 7)
                {

                }
                return;
            }
            else
            {

                client.Aisling.ClassID = responseID;
                client.Aisling.Path = (Class)responseID;

                client.SendOptionsDialog(Mundane, string.Format("You are now a {0}",
                    Convert.ToString(client.Aisling.Path)));

                if (client.Aisling.Path == Class.Priest)
                {
                    Spell.GiveTo(client.Aisling, "deo saighead");
                    Spell.GiveTo(client.Aisling, "deo saighead lamh");
                    Spell.GiveTo(client.Aisling, "beag ioc");
                    Spell.GiveTo(client.Aisling, "beag cradh");
                }

                if (client.Aisling.Path == Class.Wizard)
                {
                    Spell.GiveTo(client.Aisling, "fas nadur", 1);
                    Spell.GiveTo(client.Aisling, "beag srad", 1);
                    Spell.GiveTo(client.Aisling, "beag sal", 1);
                    Spell.GiveTo(client.Aisling, "beag puinsein", 1);
                }

                if (client.Aisling.Path == Class.Warrior)
                {
                    Skill.GiveTo(client.Aisling, "Wind Blade", 1);
                    Skill.GiveTo(client.Aisling, "Crasher", 1);
                    Skill.GiveTo(client.Aisling, "Clobber", 1);
                }

                if (client.Aisling.Path == Class.Monk)
                {
                    Skill.GiveTo(client.Aisling, "Double Punch", 1);
                    Skill.GiveTo(client.Aisling, "Ambush", 1);
                    // Skill.GiveTo(client.Aisling, "Kick");
                }

                //if (client.Aisling.Path == Class.Rogue)
                //{
                //    Skill.GiveTo(client.Aisling, "Stab and Twist");
                //    Skill.GiveTo(client.Aisling, "Throw Dagger");
                //    Skill.GiveTo(client.Aisling, "Throw");
                //    Spell.GiveTo(client.Aisling, "Needle Trap");
                //}

                client.CloseDialog();

                client.Aisling.LegendBook.AddLegend(new Legend.LegendItem
                {
                    Category = "Class",
                    Color = (byte)LegendColor.Blue,
                    Icon = (byte)LegendIcon.Victory,
                    Value = string.Format("Devoted to the path of {0} ", Convert.ToString(client.Aisling.Path))
                });

                client.Aisling.GoHome();
            }
        }
    }
}
