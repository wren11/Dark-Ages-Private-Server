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
using Darkages.Scripting;
using Darkages.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Darkages.Storage.locales.Scripts.Monsters
{
    [Script("Common Monster", "Dean")]
    public class CommonMonster : MonsterScript
    {
        private readonly Random _random = new Random();
        public List<SkillScript> SkillScripts = new List<SkillScript>();
        public List<SpellScript> SpellScripts = new List<SpellScript>();

        public SkillScript DefaultSkill
        {
            get
            {
                return SkillScripts.Find(i => i.IsScriptDefault) ?? null;
            }
        }

        public SpellScript DefaultSpell
        {
            get
            {
                return SpellScripts.Find(i => i.IsScriptDefault) ?? null;
            }
        }

        public CommonMonster(Monster monster, Area map)
            : base(monster, map)
        {

            LoadSkillScript("Assail", true);

            if (Monster.Template.SpellScripts != null)
                foreach (var spellscriptstr in Monster.Template.SpellScripts)
                {
                    LoadSpellScript(spellscriptstr);
                }

            if (Monster.Template.SkillScripts != null)
                foreach (var skillscriptstr in Monster.Template.SkillScripts)
                {
                    LoadSkillScript(skillscriptstr);
                }
        }

        private void LoadSkillScript(string skillscriptstr, bool primary = false)
        {
            try
            {
                var script = ScriptManager.Load<SkillScript>(skillscriptstr,
                    Skill.Create(1, ServerContext.GlobalSkillTemplateCache[skillscriptstr]));


                if (script != null)
                {
                    script.Skill.NextAvailableUse = DateTime.UtcNow;
                    script.IsScriptDefault = primary;
                    SkillScripts.Add(script);
                }
            }
            catch (Exception)
            {
                //ignore
            }
        }

        private void LoadSpellScript(string spellscriptstr, bool primary = false)
        {
            try
            {
                var script = ScriptManager.Load<SpellScript>(spellscriptstr,
                    Spell.Create(1, ServerContext.GlobalSpellTemplateCache[spellscriptstr]));


                if (script != null)
                {
                    script.IsScriptDefault = primary;
                    SpellScripts.Add(script);
                }
            }
            catch (Exception)
            {
                //ignore
            }
        }

        public Sprite Target
        {
            get
            {
                return Monster.Target;
            }
        }

        public override void OnApproach(GameClient client)
        {
            RefreshTarget(client);

            UpdateTarget();

        }

        private void RefreshTarget(GameClient client)
        {
            if (client.Aisling.Dead)
            {
                ClearTarget();
            }

            if (client.Aisling.Invisible)
            {
                ClearTarget();
            }
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
                ? Monster.Template.Name : Monster.Template.BaseName);
        }

        public override void OnDeath(GameClient client)
        {
            if (Monster.Target != null)
                if (Monster.Target is Aisling)
                    Monster.GenerateRewards(Monster.Target as Aisling);

            Monster.Template.SpawnCount--;

            if (Monster.Template.SpawnCount < 0)
                Monster.Template.SpawnCount = 0;

            Monster.Remove();
            Monster.Target = null;

            DelObject(Monster);
        }

        public override void OnSkulled(GameClient client)
        {
            Monster.Animate(163);
        }

        public override void OnLeave(GameClient client)
        {
            UpdateTarget();
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (Monster == null)
                return;

            if (!Monster.IsAlive)
                return;

            if (Monster.IsConfused || Monster.IsFrozen || Monster.IsParalyzed || Monster.IsSleeping)
                return;

            if (Monster.Target != null && Monster.TaggedAislings.Count > 0 && Monster.Template.EngagedWalkingSpeed > 0)
            {
                Monster.WalkTimer.Delay = TimeSpan.FromMilliseconds(Monster.Template.EngagedWalkingSpeed); 
            }

            Monster.BashTimer.Update(elapsedTime);
            Monster.CastTimer.Update(elapsedTime);
            Monster.WalkTimer.Update(elapsedTime);

            try
            {
                if (Monster.BashTimer.Elapsed)
                {
                    Monster.BashTimer.Reset();

                    if (Monster.BashEnabled)
                        Bash();
                }

                if (Monster.CastTimer.Elapsed)
                {
                    Monster.CastTimer.Reset();

                    if (Monster.CastEnabled)
                    {
                        CastSpell();
                    }
                }

                if (Monster.WalkTimer.Elapsed)
                {
                    Monster.WalkTimer.Reset();

                    if (Monster.WalkEnabled)
                    {
                        Walk();
                    }
                }
            }
            catch
            {
                //ignore
            }
        }

        private void UpdateTarget()
        {
            if (Monster.Target != null && Monster.Target is Aisling)
            {
                var aisling = Monster.Target as Aisling;

                if (aisling.Invisible || aisling.Dead || aisling.CurrentHp == 0)
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
                {
                    if (Monster.AislingsNearby().Length > 0)
                    {
                        ClearTarget();
                    }
                }
            }
            else
            {
                if (Monster.Aggressive)
                {
                    if (Monster.Target == null)
                    {

                        Monster.Target = GetObjects(Monster.Map, i => i.Serial != Monster.Serial
                        && i.WithinRangeOf(Monster) && i.CurrentHp > 0,
                        Monster.Template.MoodType == MoodQualifer.VeryAggressive ? Get.Aislings | Get.Monsters : Get.Aislings)
                                          .OrderBy(v => v.Position.DistanceFrom(Monster.Position)).FirstOrDefault();
                    }

                    if (Monster.Target != null && Monster.Target.CurrentHp <= 0)
                    {
                        Monster.Target = null;
                    }

                    Monster.WalkEnabled = Monster.Target != null;
                }
            }
        }


        private void CastSpell()
        {
            if (!Monster.CanCast)
                return;

            if (Monster.Target == null)
                return;

            if (!Monster.Target.WithinRangeOf(Monster))
                return;

            if (Monster != null && Monster.Target   != null && SpellScripts.Count > 0)
            {
                if (_random.  Next(1, 101) < ServerContext.Config.MonsterSpellSuccessRate)
                {
                    var spellidx = _random.Next(SpellScripts.Count);

                    if (SpellScripts[spellidx] != null)
                        SpellScripts[spellidx].OnUse(Monster, Target);
                }
            }

            if (Monster != null && Monster.Target != null && Monster.Target.CurrentHp > 0)
            {
                if (DefaultSpell != null)
                    DefaultSpell.OnUse(Monster, Monster.Target);
            }
        }


        public override void OnDamaged(GameClient client, int dmg)
        {
            if (Monster.Target == null || Monster.Target != client.Aisling)
            {
                if (Monster.CanAcceptTarget(client.Aisling))
                {
                    Monster.Target = client.Aisling;
                    Monster.Aggressive = true;
                }
            }
        }

        private void Walk()
        {
            if (!Monster.CanMove)
                return;

            if (Target != null)
            {
                if (Monster.NextTo(Target.X, Target.Y))
                {
                    if (Monster.Facing(Target.X, Target.Y, out int direction))
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
                    Monster.WalkTo(Target.X, Target.Y);

                }
            }
            else
            {
                Monster.BashEnabled = false;
                Monster.CastEnabled = false;

                if (Monster.Template.PathQualifer.HasFlag(PathQualifer.Patrol))
                {
                    if (Monster.Template.Waypoints == null)
                        Monster.Wander();
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

        private void Bash()
        {
            if (!Monster.CanCast)
                return;

            var obj = Monster.GetInfront(1);

            if (obj == null)
                return;


            if (Monster.Target != null)
                if (!Monster.Facing(Target.X, Target.Y, out int direction))
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

            if (Monster != null && Monster.Target != null && SkillScripts.Count > 0)
            {
                var sobj = SkillScripts.FirstOrDefault(i => i.Skill.Ready);

                if (sobj != null)
                {
                    var skill = sobj.Skill;

                    sobj?.OnUse(Monster);
                    {
                        skill.InUse = true;

                        if (skill.Template.Cooldown > 0)
                            skill.NextAvailableUse = DateTime.UtcNow.AddSeconds(skill.Template.Cooldown);
                        else
                            skill.NextAvailableUse = DateTime.UtcNow.AddMilliseconds(ServerContext.Config.GlobalBaseSkillDelay);
                    }

                    skill.InUse = false;
                }
            }

            if (Monster != null && DefaultSkill != null)
            {
                DefaultSkill.OnUse(Monster);
            }

        }

        private void ClearTarget()
        {
            Monster.CastEnabled = false;
            Monster.BashEnabled = false;
            Monster.WalkEnabled = true;
            Monster.Target = null;
        }
    }
}
