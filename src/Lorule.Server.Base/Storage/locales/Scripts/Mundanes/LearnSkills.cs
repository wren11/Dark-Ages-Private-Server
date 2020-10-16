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
    [Script("combat trainer 1")]
    public class LearnSkills : MundaneScript
    {
        public LearnSkills(GameServer server, Mundane mundane)
            : base(server, mundane)
        {
        }

        public override void OnClick(GameServer server, GameClient client)
        {
            client.SendOptionsDialog(Mundane,
                "Aha, The chosen one has finally decided to visit me. Oh Young Aisling.\nI can offer you some new tricks to conquer this realm.\nAre you ready?",
                new OptionsDataItem(0x0001, "Show Available Skills"),
                new OptionsDataItem(0x0002, "Forget Skill")
            );
        }

        public override void OnGossip(GameServer server, GameClient client, string message)
        {
        }

        public override void OnResponse(GameServer server, GameClient client, ushort responseID, string args)
        {
            var skills = ServerContext.GlobalSkillTemplateCache.Select(i => i.Value)
                .Where(i => i.NpcKey != null && i.NpcKey.Equals(Mundane.Template.Name)).ToArray();

            var availableSkillTemplates = new List<SkillTemplate>();

            foreach (var skill in skills)
            {
                if (skill.Prerequisites != null)
                    if (skill.Prerequisites.Class_Required == client.Aisling.Path)
                        availableSkillTemplates.Add(skill);

                if (skill.LearningRequirements != null &&
                    skill.LearningRequirements.TrueForAll(i => i.Class_Required == client.Aisling.Path))
                    availableSkillTemplates.Add(skill);
            }

            switch (responseID)
            {
                case 0x0001:
                    var learnedSkills = client.Aisling.SkillBook.Skills.Where(i => i.Value != null)
                        .Select(i => i.Value.Template).ToList();
                    var newSkills = availableSkillTemplates.Except(learnedSkills).ToList();

                    newSkills = newSkills.OrderBy(i =>
                        Math.Abs(i.Prerequisites.ExpLevel_Required - client.Aisling.ExpLevel)).ToList();

                    if (newSkills.Count > 0)
                    {
                        client.SendSkillLearnDialog(Mundane,
                            "Not even the Gods of Lorule posess the power these abilities can bring you.\nYou are lucky I'm even showing these to you. Choose Wisely young Aisling,\nYou have been chosen.",
                            0x0003,
                            newSkills.Where(i => i.Prerequisites.Class_Required == client.Aisling.Path));
                    }
                    else
                    {
                        client.CloseDialog();
                        client.SendMessage(0x02, "I have nothing to show you yet, return when you are stronger.");
                    }

                    break;

                case 0x0002:
                {
                    client.SendSkillForgetDialog(Mundane,
                        "What must you vanish from your mind?\nMake sure you understand, It will be gone.", 0x9000);
                }
                    break;

                case 0x9000:
                {
                    var idx = -1;
                    int.TryParse(args, out idx);

                    if (idx < 0 || idx > byte.MaxValue)
                    {
                        client.SendMessage(0x02, "Go away.");
                        client.CloseDialog();
                    }

                    client.Aisling.SkillBook.Remove((byte) idx);
                    client.Send(new ServerFormat2D((byte) idx));

                    client.SendSkillForgetDialog(Mundane,
                        "It is gone, Shall we cleanse more?\nRemember, This cannot be undone.", 0x9000);
                }
                    break;

                case 0x0003:
                    client.SendOptionsDialog(Mundane,
                        "Are you sure you want to learn " + args +
                        "?\nLet's see if you have what it takes to learn it.\nDo you wish to expand your mind? Let me examine you.",
                        args,
                        new OptionsDataItem(0x0006, $"What does {args} do?"),
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
                    }
                    else
                    {
                        var conditions = subject.Prerequisites.IsMet(client.Aisling, (msg, result) =>
                        {
                            if (!result)
                                client.SendOptionsDialog(Mundane, msg, subject.Name);
                            else
                                client.SendOptionsDialog(Mundane,
                                    "You have satisfied my requirements, Do you wish to proceed?",
                                    subject.Name,
                                    new OptionsDataItem(0x0005, "Yes, I'm Ready."),
                                    new OptionsDataItem(0x0001, "No, I'm not ready."));
                        });
                    }
                }
                    break;

                case 0x0006:
                {
                    var subject = ServerContext.GlobalSkillTemplateCache[args];
                    if (subject == null)
                        return;

                    client.SendOptionsDialog(Mundane,
                        $"{args} - {(string.IsNullOrEmpty(subject.Description) ? "No more information is available." : subject.Description)}" +
                        "\n" + subject.Prerequisites,
                        subject.Name,
                        new OptionsDataItem(0x0006, $"What does {subject.Name} do?"),
                        new OptionsDataItem(0x0004, "Yes"),
                        new OptionsDataItem(0x0001, "No"));
                }
                    break;

                case 0x0005:
                {
                    var subject = ServerContext.GlobalSkillTemplateCache[args];
                    if (subject == null)
                        return;

                    client.SendAnimation(109, client.Aisling, Mundane);
                    client.LearnSkill(Mundane, subject, "So be it; Use it wisely, chosen one.");
                }
                    break;
            }
        }

        public override void TargetAcquired(Sprite Target)
        {
        }
    }
}