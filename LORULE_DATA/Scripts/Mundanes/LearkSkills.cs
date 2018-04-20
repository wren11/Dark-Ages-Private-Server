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
using System.Linq;

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script("Learn Skills")]
    public class LearkSkills : MundaneScript
    {
        public LearkSkills(GameServer server, Mundane mundane)
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
            client.SendOptionsDialog(Mundane, "How may I assist you?",
                new OptionsDataItem(0x0001, "Learn Skill"));
        }

        public override void OnResponse(GameServer server, GameClient client, ushort responseID, string args)
        {
            switch (responseID)
            {
                // Skill Learn
                case 0x0001:
                    var skills = ServerContext.GlobalSkillTemplateCache.Select(i => i.Value)
                        .Where(i => i.Prerequisites != null && i.NpcKey.Equals(this.Mundane.Template.Name)
                        && i.Prerequisites.Class_Required == client.Aisling.Path).ToList();
                    var learned_skills = client.Aisling.SkillBook.Skills.Where(i => i.Value != null).Select(i => i.Value.Template)
                        .ToList();

                    foreach (var skill in learned_skills)
                    {
                        if (skills.Find(i => i.Name.Equals(skill.Name)) != null)
                        {
                            client.CloseDialog();
                            client.SendMessage(0x02, "Nothing available to learn right now.");
                            return;
                        }
                    }

                    if (skills.Count > 0)
                    {
                        client.SendSkillLearnDialog(Mundane, "Which skill would you like to learn?", 0x0003, 
                            skills.Where(i => i.Prerequisites.Class_Required == client.Aisling.Path));
                    }
                    else
                    {
                        client.CloseDialog();
                        client.SendMessage(0x02, "Nothing available to learn right now.");
                        return;
                    }
                    break;
                // Skill Confirmation
                case 0x0003:
                    client.SendOptionsDialog(Mundane, "Are you sure you want to learn " + args + "?", args,
                        new OptionsDataItem(0x0004, "Yes"),
                        new OptionsDataItem(0x0001, "No"));
                    break;
                case 0x0004:
                    {

                        var subject = ServerContext.GlobalSkillTemplateCache[args];
                        if (subject == null)
                            return;

                        if (subject.Prerequisites == null)
                        {
                            client.CloseDialog();
                            client.SendMessage(0x02, ServerContext.Config.CantDoThat);
                            return;
                        }
                        else
                        {
                            var conditions = subject.Prerequisites.IsMet(client.Aisling, (msg, result) =>
                            {
                                if (!result)
                                {
                                    client.SendOptionsDialog(Mundane, msg, subject.Name);
                                }
                                else
                                {
                                    client.SendOptionsDialog(Mundane, "You are able to learn this skill, Do you wish to proceed?",
                                        subject.Name,
                                        new OptionsDataItem(0x0005, string.Format("Yes, Learn {0}", subject.Name)),
                                        new OptionsDataItem(0x0001, "No"));
                                }
                            });
                        }
                    }
                    break;
                // Skill Acquire
                case 0x0005:
                    {
                        var subject = ServerContext.GlobalSkillTemplateCache[args];
                        if (subject == null)
                            return;

                        client.LearnSkill(Mundane, subject, "So be it.");

                    }
                    break;
            }
        }
    }
}
