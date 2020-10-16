#region

using System.Collections.Generic;
using System.Linq;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script("The Grand Master")]
    public class GrandMaster01 : MundaneScript
    {
        public GrandMaster01(GameServer server, Mundane mundane)
            : base(server, mundane)
        {
        }

        public override void OnClick(GameServer server, GameClient client)
        {
            client.SendOptionsDialog(Mundane, "\nHello, How may i help you?",
                new OptionsDataItem(0x0001, "Learn Secret"),
                new OptionsDataItem(0x0002, "Forget Secret")
            );
        }

        public override void OnGossip(GameServer server, GameClient client, string message)
        {
        }

        public override void OnResponse(GameServer server, GameClient client, ushort responseID, string args)
        {
            var spells = ServerContext.GlobalSpellTemplateCache.Select(i => i.Value)
                .Where(i => i.NpcKey != null && i.NpcKey.Equals(Mundane.Template.Name)).ToArray();

            var availableSpellTemplates = new List<SpellTemplate>();

            foreach (var skill in spells)
            {
                if (skill.Prerequisites != null)
                    if (skill.Prerequisites.Class_Required == client.Aisling.Path)
                        availableSpellTemplates.Add(skill);

                if (skill.LearningRequirements != null &&
                    skill.LearningRequirements.TrueForAll(i => i.Class_Required == client.Aisling.Path))
                    availableSpellTemplates.Add(skill);
            }

            switch (responseID)
            {
                case 0x0001:
                    var learnedSpells = client.Aisling.SpellBook.Spells.Where(i => i.Value != null)
                        .Select(i => i.Value.Template).ToList();
                    var newSpells = availableSpellTemplates.Except(learnedSpells).ToList();

                    if (newSpells.Count > 0)
                    {
                        client.SendSpellLearnDialog(Mundane, "Only the dedicated can unlock the power of magic.",
                            0x0003,
                            newSpells.Where(i => i.Prerequisites.Class_Required == client.Aisling.Path));
                    }
                    else
                    {
                        client.CloseDialog();
                        client.SendMessage(0x02, "I have nothing to teach for now.");
                    }

                    break;

                case 0x0002:
                {
                    client.SendSpellForgetDialog(Mundane,
                        "What must you vanish from your mind?\nMake sure you understand, This cannot be un-done.",
                        0x9000);
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

                    client.Aisling.SpellBook.Remove((byte) idx);
                    client.Send(new ServerFormat18((byte) idx));

                    client.SendSpellForgetDialog(Mundane,
                        "It is gone, Shall we cleanse more?\nRemember, This cannot be un-done.", 0x9000);
                }
                    break;

                case 0x0003:
                    client.SendOptionsDialog(Mundane,
                        "Are you sure you want to learn " + args +
                        "?\nDo you wish to expand your mind? Let me check if you're ready.", args,
                        new OptionsDataItem(0x0006, $"What does {args} do?"),
                        new OptionsDataItem(0x0004, "Yes"),
                        new OptionsDataItem(0x0001, "No"));
                    break;

                case 0x0004:
                {
                    var subject = ServerContext.GlobalSpellTemplateCache[args];
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
                                    "You have satisfied my requirements, Ready for such magic?",
                                    subject.Name,
                                    new OptionsDataItem(0x0005, "Yes, I'm Ready."),
                                    new OptionsDataItem(0x0001, "No, I'm not ready."));
                        });
                    }
                }
                    break;

                case 0x0006:
                {
                    var subject = ServerContext.GlobalSpellTemplateCache[args];
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
                    var subject = ServerContext.GlobalSpellTemplateCache[args];
                    if (subject == null)
                        return;

                    client.SendAnimation(109, client.Aisling, Mundane);
                    client.LearnSpell(Mundane, subject, "Go Spark, Aisling.");
                }
                    break;
            }
        }

        public override void TargetAcquired(Sprite Target)
        {
        }
    }
}