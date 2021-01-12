using System.Linq;
using Darkages.Network.ServerFormats;
using Darkages.Network.ClientFormats;
using Darkages.Scripting;
using Darkages.Templates;
using Darkages.Common;
using Darkages.Compression;
using Darkages.IO;
using Darkages.Types;
using System.Collections.Concurrent;
using System.Collections;
using Darkages;
using Darkages.Storage.locales.Buffs;
using Darkages.Storage.locales.debuffs;
using System.Collections.Generic;
using System;
using Darkages.Network.Game;


[Script("New Skill", "Test")]
public class NewSkill : SkillScript
{
    private Skill _skill;
    
    public NewSkill(Skill skill) : base(skill)
    {
        _skill = skill;
    }

    public override void OnFailed(Sprite sprite)
    {

    }

    public override void OnSuccess(Sprite sprite)
    {

    }

    public override void OnUse(Sprite sprite)
    {

    }
}
