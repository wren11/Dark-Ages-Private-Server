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
    [Script("test")]
    public class FortyOneArmor : MundaneScript
    {
        public FortyOneArmor(GameServer server, Mundane mundane)
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
            var gms = ServerContext.Config.GameMasters;
            var displayed = false;

            foreach (var gm in gms)
            {
                if (displayed)
                    return;

                if (client.Aisling.Username.ToLower() == gm.ToLower())
                {
                    var opts = new List<OptionsDataItem>();
                    opts.Add(new OptionsDataItem(0x0111, "Wipe Skills"));
                    opts.Add(new OptionsDataItem(0x0001, "Warrior Build"));
                    opts.Add(new OptionsDataItem(0x0002, "Rogue Build"));
                    opts.Add(new OptionsDataItem(0x0003, "Wizard Build"));
                    opts.Add(new OptionsDataItem(0x0004, "Priest Build"));
                    opts.Add(new OptionsDataItem(0x0005, "Monk Build"));
                    opts.Add(new OptionsDataItem(0x0006, "Peasant Build"));

                    client.SendOptionsDialog(Mundane, "Greetings, ready to make an armor?", opts.ToArray());
                    displayed = true;
                }
            }
        }

        public override void OnResponse(GameServer server, GameClient client, ushort responseID, string args)
        {
            switch (responseID)
            {
                case 0x0111:

                    Skill.GiveTo(client.Aisling, "Throw");
                    Skill.GiveTo(client.Aisling, "Assail");
                    Skill.GiveTo(client.Aisling, "Look");
                    Spell.GiveTo(client.Aisling, "GM Spell");
                    Spell.GiveTo(client.Aisling, "Create Item");
                    Spell.GiveTo(client.Aisling, "Blind Me");

                    client.SendOptionsDialog(Mundane, "Cleared.");

                    break;

                case 0x0001:

                    var list = ServerContext.GlobalSkillTemplateCache
                        .Where(i => i.Value.Prerequisites != null &&
                                    i.Value.Prerequisites.Class_Required == Class.Warrior).Select(i => i.Value.Name)
                        .ToList();
                    var slist = ServerContext.GlobalSpellTemplateCache
                        .Where(i => i.Value.Prerequisites != null &&
                                    i.Value.Prerequisites.Class_Required == Class.Warrior).Select(i => i.Value.Name)
                        .ToList();
                    var book = client.Aisling.SkillBook.Skills.Where(i => i.Value != null).Select(i => i.Value.Template)
                        .ToList();
                    var sbook = client.Aisling.SpellBook.Spells.Where(i => i.Value != null)
                        .Select(i => i.Value.Template).ToList();

                    if (book is null)
                        return;
                    if (sbook is null)
                        return;

                    foreach (var i in book)
                    {
                        if (i == null)
                            continue;
                        if (i != null)
                        {
                            client.ForgetSkill(i.Name);
                            client.Refresh(true);
                        }
                    }

                    foreach (var i in sbook)
                    {
                        if (i == null)
                            continue;

                        if (i != null)
                        {
                            client.ForgetSpell(i.Name);
                            client.Refresh(true);
                        }
                    }

                    foreach (var i in list)
                    {
                        if (i == null)
                            continue;

                        Skill.GiveTo(client.Aisling, i);
                        client.Refresh(true);
                    }

                    foreach (var i in slist)
                    {
                        if (i is null)
                            continue;

                        Spell.GiveTo(client.Aisling, i);
                        client.Refresh(true);
                    }

                    Skill.GiveTo(client.Aisling, "Throw");
                    Skill.GiveTo(client.Aisling, "Assail");
                    Skill.GiveTo(client.Aisling, "Look");
                    Spell.GiveTo(client.Aisling, "GM Spell");
                    Spell.GiveTo(client.Aisling, "Create Item");
                    Spell.GiveTo(client.Aisling, "Blind Me");
                    client.Aisling.Path = Class.Warrior;
                    //client.Aisling.ClassID = 1;
                    client.SendOptionsDialog(Mundane, "You are now a Warrior.");

                    break;

                case 0x0002:
                    var list2 = ServerContext.GlobalSkillTemplateCache
                        .Where(i => i.Value.Prerequisites != null &&
                                    i.Value.Prerequisites.Class_Required == Class.Rogue).Select(i => i.Value.Name)
                        .ToList();
                    var slist2 = ServerContext.GlobalSpellTemplateCache
                        .Where(i => i.Value.Prerequisites != null &&
                                    i.Value.Prerequisites.Class_Required == Class.Rogue).Select(i => i.Value.Name)
                        .ToList();
                    var book2 = client.Aisling.SkillBook.Skills.Where(i => i.Value != null)
                        .Select(i => i.Value.Template).ToList();
                    var sbook2 = client.Aisling.SpellBook.Spells.Where(i => i.Value != null)
                        .Select(i => i.Value.Template).ToList();

                    if (book2 is null)
                        return;
                    if (sbook2 is null)
                        return;

                    foreach (var i in book2)
                    {
                        if (i == null)
                            continue;

                        client.ForgetSkill(i.Name);
                    }

                    foreach (var i in sbook2)
                    {
                        if (i == null)
                            continue;

                        client.ForgetSpell(i.Name);
                    }

                    foreach (var i in list2)
                    {
                        if (i == null)
                            continue;

                        Skill.GiveTo(client.Aisling, i);
                    }

                    foreach (var i in slist2)
                    {
                        if (i is null)
                            continue;

                        Spell.GiveTo(client.Aisling, i);
                    }

                    Skill.GiveTo(client.Aisling, "Throw");
                    Skill.GiveTo(client.Aisling, "Assail");
                    Skill.GiveTo(client.Aisling, "Look");
                    Spell.GiveTo(client.Aisling, "GM Spell");
                    Spell.GiveTo(client.Aisling, "Create Item");
                    Spell.GiveTo(client.Aisling, "Blind Me");

                    client.SendOptionsDialog(Mundane, "You are now a Rogue.");
                    client.Aisling.Path = Class.Rogue;
                    //client.Aisling.ClassID = 2;

                    break;

                case 0x0003:
                    var list3 = ServerContext.GlobalSkillTemplateCache
                        .Where(i => i.Value.Prerequisites != null &&
                                    i.Value.Prerequisites.Class_Required == Class.Wizard).Select(i => i.Value.Name)
                        .ToList();
                    var slist3 = ServerContext.GlobalSpellTemplateCache
                        .Where(i => i.Value.Prerequisites != null &&
                                    i.Value.Prerequisites.Class_Required == Class.Wizard).Select(i => i.Value.Name)
                        .ToList();
                    var book3 = client.Aisling.SkillBook.Skills.Where(i => i.Value != null)
                        .Select(i => i.Value.Template).ToList();
                    var sbook3 = client.Aisling.SpellBook.Spells.Where(i => i.Value != null)
                        .Select(i => i.Value.Template).ToList();

                    if (book3 is null)
                        return;
                    if (sbook3 is null)
                        return;

                    foreach (var i in book3)
                    {
                        if (i == null)
                            continue;

                        client.ForgetSkill(i.Name);
                    }

                    foreach (var i in sbook3)
                    {
                        if (i == null)
                            continue;

                        client.ForgetSpell(i.Name);
                    }

                    foreach (var i in list3)
                    {
                        if (i == null)
                            continue;

                        Skill.GiveTo(client.Aisling, i);
                    }

                    foreach (var i in slist3)
                    {
                        if (i is null)
                            continue;

                        Spell.GiveTo(client.Aisling, i);
                    }

                    Skill.GiveTo(client.Aisling, "Throw");
                    Skill.GiveTo(client.Aisling, "Assail");
                    Skill.GiveTo(client.Aisling, "Look");
                    Spell.GiveTo(client.Aisling, "GM Spell");
                    Spell.GiveTo(client.Aisling, "Create Item");
                    Spell.GiveTo(client.Aisling, "Blind Me");

                    client.SendOptionsDialog(Mundane, "You are now a Wizard.");
                    client.Aisling.Path = Class.Wizard;
                    //client.Aisling.ClassID = 3;


                    break;

                case 0x0004:
                    var list4 = ServerContext.GlobalSkillTemplateCache
                        .Where(i => i.Value.Prerequisites != null &&
                                    i.Value.Prerequisites.Class_Required == Class.Priest).Select(i => i.Value.Name)
                        .ToList();
                    var slist4 = ServerContext.GlobalSpellTemplateCache
                        .Where(i => i.Value.Prerequisites != null &&
                                    i.Value.Prerequisites.Class_Required == Class.Priest).Select(i => i.Value.Name)
                        .ToList();
                    var book4 = client.Aisling.SkillBook.Skills.Where(i => i.Value != null)
                        .Select(i => i.Value.Template).ToList();
                    var sbook4 = client.Aisling.SpellBook.Spells.Where(i => i.Value != null)
                        .Select(i => i.Value.Template).ToList();

                    if (book4 is null)
                        return;
                    if (sbook4 is null)
                        return;

                    foreach (var i in book4)
                    {
                        if (i == null)
                            continue;

                        client.ForgetSkill(i.Name);
                    }

                    foreach (var i in sbook4)
                    {
                        if (i == null)
                            continue;

                        client.ForgetSpell(i.Name);
                    }

                    foreach (var i in list4)
                    {
                        if (i == null)
                            continue;

                        Skill.GiveTo(client.Aisling, i);
                    }

                    foreach (var i in slist4)
                    {
                        if (i is null)
                            continue;

                        Spell.GiveTo(client.Aisling, i);
                    }

                    Skill.GiveTo(client.Aisling, "Throw");
                    Skill.GiveTo(client.Aisling, "Assail");
                    Skill.GiveTo(client.Aisling, "Look");
                    Spell.GiveTo(client.Aisling, "GM Spell");
                    Spell.GiveTo(client.Aisling, "Create Item");
                    Spell.GiveTo(client.Aisling, "Blind Me");

                    client.SendOptionsDialog(Mundane, "You are now a Priest.");
                    client.Aisling.Path = Class.Priest;
                    //client.Aisling.ClassID = 4;

                    break;

                case 0x0005:
                    var list5 = ServerContext.GlobalSkillTemplateCache
                        .Where(i => i.Value.Prerequisites != null && i.Value.Prerequisites.Class_Required == Class.Monk)
                        .Select(i => i.Value.Name).ToList();
                    var slist5 = ServerContext.GlobalSpellTemplateCache
                        .Where(i => i.Value.Prerequisites != null && i.Value.Prerequisites.Class_Required == Class.Monk)
                        .Select(i => i.Value.Name).ToList();
                    var book5 = client.Aisling.SkillBook.Skills.Where(i => i.Value != null)
                        .Select(i => i.Value.Template).ToList();
                    var sbook5 = client.Aisling.SpellBook.Spells.Where(i => i.Value != null)
                        .Select(i => i.Value.Template).ToList();

                    if (book5 is null)
                        return;
                    if (sbook5 is null)
                        return;

                    foreach (var i in book5)
                    {
                        if (i == null)
                            continue;

                        client.ForgetSkill(i.Name);
                    }

                    foreach (var i in sbook5)
                    {
                        if (i == null)
                            continue;

                        client.ForgetSpell(i.Name);
                    }

                    foreach (var i in list5)
                    {
                        if (i == null)
                            continue;

                        Skill.GiveTo(client.Aisling, i);
                    }

                    foreach (var i in slist5)
                    {
                        if (i is null)
                            continue;

                        Spell.GiveTo(client.Aisling, i);
                    }

                    Skill.GiveTo(client.Aisling, "Throw");
                    Skill.GiveTo(client.Aisling, "Assail");
                    Skill.GiveTo(client.Aisling, "Look");
                    Spell.GiveTo(client.Aisling, "GM Spell");
                    Spell.GiveTo(client.Aisling, "Create Item");
                    Spell.GiveTo(client.Aisling, "Blind Me");

                    client.SendOptionsDialog(Mundane, "You are now a Monk.");
                    client.Aisling.Path = Class.Monk;
                    //client.Aisling.ClassID = 5;

                    break;

                case 0x0006:
                    var book6 = client.Aisling.SkillBook.Skills.Where(i => i.Value != null)
                        .Select(i => i.Value.Template).ToList();
                    var sbook6 = client.Aisling.SpellBook.Spells.Where(i => i.Value != null)
                        .Select(i => i.Value.Template).ToList();
                    var booked6 = book6.ToString();
                    var sbooked6 = sbook6.ToString();

                    if (book6 is null)
                        return;
                    if (sbook6 is null)
                        return;

                    foreach (var i in book6)
                    {
                        if (i == null)
                            continue;

                        client.ForgetSkill(i.Name);
                    }

                    foreach (var i in sbook6)
                    {
                        if (i == null)
                            continue;

                        client.ForgetSpell(i.Name);
                    }

                    Skill.GiveTo(client.Aisling, "Assail");
                    Skill.GiveTo(client.Aisling, "Throw");
                    Skill.GiveTo(client.Aisling, "Look");
                    Spell.GiveTo(client.Aisling, "GM Spell");
                    Spell.GiveTo(client.Aisling, "Create Item");
                    Spell.GiveTo(client.Aisling, "Blind Me");

                    client.Aisling.Path = Class.Peasant;
                    //client.Aisling.ClassID = 0;

                    break;
            }
        }
    }
}