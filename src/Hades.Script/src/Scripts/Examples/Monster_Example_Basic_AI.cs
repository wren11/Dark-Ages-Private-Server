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

[Script("Common Monster", "Dean")]
public class CommonMonster : MonsterScript
{
    private readonly Random _random = new Random();
    private readonly List<SkillScript> _skillScripts = new List<SkillScript>();
    private readonly List<SpellScript> _spellScripts = new List<SpellScript>();

    public CommonMonster(Monster monster, Area map)
        : base(monster, map)
    {
        LoadSkillScript("Assail", true);

        if (Monster.Template.SpellScripts != null)
            foreach (var spellScriptStr in Monster.Template.SpellScripts)
                LoadSpellScript(spellScriptStr);

        if (Monster.Template.SkillScripts != null)
            foreach (var skillScriptStr in Monster.Template.SkillScripts)
                LoadSkillScript(skillScriptStr);
    }

    private SpellScript DefaultSpell
    {
        get { return _spellScripts.Find(i => i.IsScriptDefault) ?? null; }
    }

    private Sprite Target => Monster.Target;

    public override void OnApproach(GameClient client)
    {
        // Bug: Targets wouldn't be visible if they didn't have a template or map... 
        //if (Monster.Template == null || Monster.Map == null) return;
        RefreshTarget(client);
        UpdateTarget();
    }

    public override void OnAttacked(GameClient client)
    {
        if (client == null)
            return;

        if (client.Aisling.Dead)
            return;

        Monster.Target = client.Aisling;
    }

    public override void OnCast(GameClient client)
    {
        if (client.Aisling.Dead)
            return;

        Monster.Target = client.Aisling;
        Bash();
        CastSpell();
    }

    public override void OnClick(GameClient client)
    {
        client.SendMessage(0x02, string.IsNullOrEmpty(Monster.Template.BaseName)
            ? Monster.Template.Name
            : Monster.Template.BaseName);
    }

    public override void OnDamaged(GameClient client, int dmg, Sprite source)
    {
        if (Monster.Target == null || Monster.Target != client.Aisling)
        {
            Monster.Target = client.Aisling;
            Monster.Aggressive = true;
        }
    }

    public override void OnDeath(GameClient client)
    {
        if (Monster.Target is Aisling)
            Monster.GenerateRewards(Monster.Target as Aisling);

        Monster.Remove();
        Monster.Target = null;

        DelObject(Monster);
    }

    public override void OnLeave(GameClient client)
    {
        UpdateTarget();
    }

    public override void OnSkulled(GameClient client)
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (Monster == null)
            return;

        if (!Monster.IsAlive)
            return;

        if (Monster.IsConfused || Monster.IsFrozen || Monster.IsParalyzed || Monster.IsSleeping)
            return;

        HandleMonsterState(elapsedTime);
    }

    private void Bash()
    {
        if (!Monster.CanCast)
            return;

        var obj = Monster.GetInfront();

        if (obj == null)
            return;

        if (Monster.Target != null)
            if (!Monster.Facing(Target.XPos, Target.YPos, out var direction))
            {
                Monster.Direction = (byte)direction;
                Monster.Turn();
                return;
            }

        if (Target == null || Target.CurrentHp == 0)
        {
            ClearTarget();
            return;
        }

        if (Monster.Target != null)
            if (!Monster.Facing(Target.XPos, Target.YPos, out var direction))
            {
                Monster.Direction = (byte)direction;
                Monster.Turn();
                return;
            }

        if (Monster?.Target == null || _skillScripts.Count <= 0) return;
        var sObj = _skillScripts.FirstOrDefault(i => i.Skill.Ready);

        if (sObj == null) return;
        var skill = sObj.Skill;

        sObj.OnUse(Monster);
        {
            skill.InUse = true;

            if (skill.Template.Cooldown > 0)
                skill.NextAvailableUse = DateTime.UtcNow.AddSeconds(skill.Template.Cooldown);
            else
                skill.NextAvailableUse =
                    DateTime.UtcNow.AddMilliseconds(Monster.Template.AttackSpeed > 0
                        ? Monster.Template.AttackSpeed
                        : 1500);
        }

        skill.InUse = false;
    }

    private void CastSpell()
    {
        if (!Monster.CanCast)
            return;

        if (Monster.Target == null)
            return;

        if (!Monster.Target.WithinRangeOf(Monster))
            return;

        if (Monster != null && Monster.Target != null && _spellScripts.Count > 0)
            if (_random.Next(1, 101) < ServerContext.Config.MonsterSpellSuccessRate)
            {
                var spellidx = _random.Next(_spellScripts.Count);

                if (_spellScripts[spellidx] != null)
                    _spellScripts[spellidx].OnUse(Monster, Target);
            }

        if (Monster != null && Monster.Target != null && Monster.Target.CurrentHp > 0)
            if (DefaultSpell != null)
                DefaultSpell.OnUse(Monster, Monster.Target);
    }

    private void ClearTarget()
    {
        Monster.CastEnabled = false;
        Monster.BashEnabled = false;
        Monster.WalkEnabled = true;
        Monster.Target = null;
    }

    private void HandleMonsterState(TimeSpan elapsedTime)
    {
        if (Monster.Target != null && Monster.TaggedAislings.Count > 0 && Monster.Template.EngagedWalkingSpeed > 0)
            Monster.WalkTimer.Delay = TimeSpan.FromMilliseconds(Monster.Template.EngagedWalkingSpeed);

        if (Monster.Target is Aisling aisling)
            if (aisling.Invisible)
            {
                ClearTarget();
                Monster.WalkTimer.Update(elapsedTime);
                return;
            }

        var a = Monster.BashTimer.Update(elapsedTime);
        var b = Monster.CastTimer.Update(elapsedTime);
        var c = Monster.WalkTimer.Update(elapsedTime);


        try
        {
            if (a)
                if (Monster.BashEnabled)
                    Bash();

            if (b)
                if (Monster.CastEnabled)
                    CastSpell();

            if (!c) return;
            if (Monster.WalkEnabled)
                Walk();
        }
        catch (Exception)
        {
            //ignore
        }

        UpdateTarget();
    }

    private void LoadSkillScript(string skillScriptStr, bool primary = false)
    {
        try
        {
            if (!ServerContext.GlobalSkillTemplateCache.ContainsKey(skillScriptStr)) return;
            var scripts = ScriptManager.Load<SkillScript>(skillScriptStr,
                Skill.Create(1, ServerContext.GlobalSkillTemplateCache[skillScriptStr]));

            foreach (var script in scripts.Values.Where(script => script != null))
            {
                script.Skill.NextAvailableUse = DateTime.UtcNow;
                script.IsScriptDefault = primary;
                _skillScripts.Add(script);
            }
        }
        catch (Exception)
        {
            //ignore
        }
    }

    private void LoadSpellScript(string spellScriptStr, bool primary = false)
    {
        try
        {
            if (!ServerContext.GlobalSpellTemplateCache.ContainsKey(spellScriptStr)) return;
            var scripts = ScriptManager.Load<SpellScript>(spellScriptStr,
                Spell.Create(1, ServerContext.GlobalSpellTemplateCache[spellScriptStr]));

            foreach (var script in scripts.Values.Where(script => script != null))
            {
                script.IsScriptDefault = primary;
                _spellScripts.Add(script);
            }
        }
        catch (Exception)
        {
            //ignore
        }
    }

    private void RefreshTarget(IGameClient client)
    {
        if (client.Aisling.Dead) ClearTarget();

        if (client.Aisling.Invisible) ClearTarget();
    }

    private void UpdateTarget()
    {
        if (Monster.Target is Aisling aisling)
        {
            if (aisling!.Invisible || aisling.Dead || aisling.CurrentHp == 0)
            {
                ClearTarget();
                return;
            }
        }

        if (Monster.Target != null)
        {
            if (Monster.Target.CurrentHp == 0)
                ClearTarget();

            if (!Monster.WithinRangeOf(Monster.Target))
                ClearTarget();

            if (Monster.Target is Monster)
                if (Monster.AislingsNearby().Length > 0)
                    ClearTarget();
        }
        else
        {
            if (!Monster.Aggressive) return;
            Monster.Target ??= GetObjects(Monster.Map, i => i.WithinRangeOf(Monster), Get.Aislings)
                .Where(n => n != null)
                .OrderBy(v => v.Position.DistanceFrom(Monster.Position.X, Monster.Position.Y))
                .FirstOrDefault();

            if (Monster.Target != null && Monster.Target.CurrentHp <= 0) Monster.Target = null;

            Monster.WalkEnabled = Monster.Target != null;
        }
    }

    private void Walk()
    {
        if (!Monster.CanMove)
            return;

        if (Target != null)
        {
            if (Monster.NextTo(Target.XPos, Target.YPos))
            {
                if (Monster.Facing(Target.XPos, Target.YPos, out var direction))
                {
                    Bash();
                    Monster.BashEnabled = true;
                    Monster.CastEnabled = true;
                }
                else
                {
                    Monster.BashEnabled = false;
                    Monster.CastEnabled = true;
                    Monster.Direction = (byte)direction;
                    Monster.Turn();
                }
            }
            else
            {
                Monster.BashEnabled = false;
                Monster.CastEnabled = true;

                if (!Monster.WalkTo(Target.XPos, Target.YPos)) Monster.Wander();
            }
        }
        else
        {
            Monster.BashEnabled = false;
            Monster.CastEnabled = false;

            if (Monster.Template.PathQualifer.HasFlag(PathQualifer.Patrol))
            {
                if (Monster.Template.Waypoints == null)
                {
                    Monster.Wander();
                }
                else
                {
                    if (Monster.Template.Waypoints.Count > 0)
                        Monster.Patrol();
                    else
                        Monster.Wander();
                }
            }
            else
            {
                Monster.Wander();
            }
        }
    }
}
