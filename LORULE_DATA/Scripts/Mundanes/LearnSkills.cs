using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script("Learn Skills")]
    public class TestMundane : MundaneScript
    {
        public TestMundane(GameServer server, Mundane mundane)
            : base(server, mundane)
        {

        }

        public override void OnClick(GameServer server, GameClient client)
        {
            client.SendOptionsDialog(base.Mundane, "How may I assist you?",
                new OptionsDataItem(0x0001, "Learn Skill"));
        }
        public override void OnResponse(GameServer server, GameClient client, short responseID, string args)
        {
            switch (responseID)
            {
                // Skill Learn
                case 0x0001:
                    var skills = ServerContext.GlobalSkillTemplateCache.Values;
                    client.SendSkillLearnDialog(base.Mundane, "Which skill would you like to learn?", 0x0003, skills);
                    break;
                // Skill Confirmation
                case 0x0003:
                    client.SendOptionsDialog(base.Mundane, "Are you sure you want to learn " + args + "?", args,
                        new OptionsDataItem(0x0005, "Yes"),
                        new OptionsDataItem(0x0001, "No"));
                    break;
                // Skill Acquire
                case 0x0005:
                    var skillTemplate = ServerContext.GlobalSkillTemplateCache[args];
                    var slot = 0;

                    for (int i = 0; i < client.Aisling.SkillBook.Length; i++)
                    {
                        if (client.Aisling.SkillBook.Skills[i + 1] == null)
                        {
                            slot = (i + 1);
                            break;
                        }
                    }

                    var skill = Skill.Create(slot, skillTemplate);
                    skill.Script = ScriptManager.Load<SkillScript>(skill.Template.ScriptName, skill);
                    client.Aisling.SkillBook.Assign(skill);
                    client.Aisling.SkillBook.Set(skill, false);
                    client.Aisling.Show(Scope.NearbyAislings, new ServerFormat29((uint)client.Aisling.Serial, (uint)Mundane.Serial, 0, 124, 64));
                    client.Send(new ServerFormat2C(skill.Slot, skill.Icon, skill.Name));
                    client.SendOptionsDialog(base.Mundane, "Use this new skill wisely.");
                    break;
            }
        }
    }
}
