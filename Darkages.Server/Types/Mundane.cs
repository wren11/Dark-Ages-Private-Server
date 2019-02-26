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
using Darkages.Common;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Darkages.Types
{
    public class Mundane : Sprite
    {
        public MundaneTemplate Template { get; set; }

        [JsonIgnore] public MundaneScript Script { get; set; }

        [JsonIgnore]
        public List<SkillScript> SkillScripts = new List<SkillScript>();

        [JsonIgnore]
        public List<SpellScript> SpellScripts = new List<SpellScript>();

        [JsonIgnore]
        public SkillScript DefaultSkill => SkillScripts.Find(i => i.IsScriptDefault) ?? null;

        [JsonIgnore]
        public SpellScript DefaultSpell => SpellScripts.Find(i => i.IsScriptDefault) ?? null;

        [JsonIgnore]
        public int WaypointIndex = 0;

        [JsonIgnore]
        public Position CurrentWaypoint => Template.Waypoints[WaypointIndex] ?? null;

        [JsonIgnore]
        int ChatIdx = new ThreadLocal<int>(() => 0, true).Value;


        public void InitMundane()
        {
            if (Template.Spells != null)
                foreach (var spellscriptstr in Template.Spells)
                {
                    LoadSpellScript(spellscriptstr);
                }

            if (Template.Skills != null)
                foreach (var skillscriptstr in Template.Skills)
                {
                    LoadSkillScript(skillscriptstr);
                }

            LoadSkillScript("Assail", true);
        }

        public static void Create(MundaneTemplate template)
        {
            if (template == null)
                return;

            var map      = ServerContext.GlobalMapCache[template.AreaID];
            var existing = template.GetObject<Mundane>(map, p => p != null && p.Template != null && p.Template.Name == template.Name);

            //this npc was already created?
            if (existing != null)
                if (existing.CurrentHp == 0)
                    existing.OnDeath();
                else
                    return;


            var npc = new Mundane
            {
                Template = template
            };

            if (npc.Template.TurnRate == 0)
                npc.Template.TurnRate = 5;

            if (npc.Template.CastRate == 0)
                npc.Template.CastRate = 2;

            if (npc.Template.WalkRate == 0)
                npc.Template.WalkRate = 2;


            npc.CurrentMapId = npc.Template.AreaID;
            lock (Generator.Random)
            {
                npc.Serial = Generator.GenerateNumber();
            }

            npc.X = template.X;
            npc.Y = template.Y;
            npc._MaximumHp = (int)(template.Level / 0.1 * 15);
            npc._MaximumMp = (int)(template.Level / 0.1 * 5);
            npc.Template.MaximumHp = npc.MaximumHp;
            npc.Template.MaximumMp = npc.MaximumMp;

            npc.CurrentHp = npc.Template.MaximumHp;
            npc.CurrentMp = npc.Template.MaximumMp;
            npc.Direction = npc.Template.Direction;
            npc.CurrentMapId = npc.Template.AreaID;

            npc.BonusAc = (int)((5 + npc.Template.Level - 40 / 100 * npc.Template.Level));

            if (npc.BonusAc < 0 || npc.BonusAc >= 100)
            {
                npc.BonusAc = 100;
            }

            npc.DefenseElement = Generator.RandomEnumValue<ElementManager.Element>();
            npc.OffenseElement = Generator.RandomEnumValue<ElementManager.Element>();

            npc.Script = ScriptManager.Load<MundaneScript>(template.ScriptKey, ServerContext.Game, npc);

            npc.Template.AttackTimer = new GameServerTimer(TimeSpan.FromMilliseconds(450));
            npc.Template.EnableTurning = false;
            npc.Template.WalkTimer = new GameServerTimer(TimeSpan.FromSeconds(npc.Template.WalkRate));
            npc.Template.ChatTimer = new GameServerTimer(TimeSpan.FromSeconds(Generator.Random.Next(25, 40)));
            npc.Template.TurnTimer = new GameServerTimer(TimeSpan.FromSeconds(npc.Template.TurnRate));
            npc.Template.SpellTimer = new GameServerTimer(TimeSpan.FromSeconds(npc.Template.CastRate));
            npc.InitMundane();
            npc.AddObject(npc);
        }

        public void LoadSkillScript(string skillscriptstr, bool primary = false)
        {
            Skill obj;
            var script = ScriptManager.Load<SkillScript>(skillscriptstr,
                obj = Skill.Create(1, ServerContext.GlobalSkillTemplateCache[skillscriptstr]));

            obj.NextAvailableUse = DateTime.UtcNow;

            if (script != null)
            {
                script.Skill = obj;
                script.IsScriptDefault = primary;
                SkillScripts.Add(script);
            }
        }

        private void LoadSpellScript(string spellscriptstr, bool primary = false)
        {
            var script = ScriptManager.Load<SpellScript>(spellscriptstr,
                Spell.Create(1, ServerContext.GlobalSpellTemplateCache[spellscriptstr]));


            if (script != null)
            {
                script.IsScriptDefault = primary;
                SpellScripts.Add(script);
            }
        }

        public void OnDeath()
        {
            Map.Update(X, Y, this, true);

            RemoveActiveTargets();

            if (CurrentHp == 0)
                new TaskFactory().StartNew(() =>
                {
                    Thread.Sleep(1000);
                    Remove<Mundane>();
                });
        }

        private void RemoveActiveTargets()
        {
            var nearbyMonsters = GetObjects<Monster>(Map, i => WithinRangeOf(this));
            foreach (var nearby in nearbyMonsters)
                if (nearby.Target != null && nearby.Target.Serial == Serial)
                {
                    nearby.Target = null;
                }
        }

        public void Update(TimeSpan update)
        {
            if (Template == null)
                return;

            if (IsConfused || IsFrozen || IsParalyzed || IsSleeping)
                return;

            if (Target != null && !WithinRangeOf(Target))
            {
                Target = null;
            }

            if (Template.ChatTimer != null)
            {
                Template.ChatTimer.Update(update);

                if (Template.ChatTimer.Elapsed)
                {
                    var nearby = GetObjects<Aisling>(Map, i => i.WithinRangeOf(this));
                    foreach (var obj in nearby)
                    {
                        if (Template.Speech.Count > 0)
                        {
                            var msg = Template.Speech[ChatIdx++ % Template.Speech.Count];

                            obj.Show(Scope.Self,
                                new ServerFormat0D
                                {
                                    Serial = Serial,
                                    Text = msg,
                                    Type = 0
                                });

                        }
                    }

                    Template.ChatTimer.Reset();
                }
            }

            if (Template.EnableTurning)
            {
                if (Template.TurnTimer != null)
                {
                    Template.TurnTimer.Update(update);
                    if (Template.TurnTimer.Elapsed)
                    {
                        lock (Generator.Random)
                        {
                            Direction = (byte)Generator.Random.Next(0, 4);
                        }

                        Turn();

                        Template.TurnTimer.Reset();
                    }
                }
            }

            if (Template.EnableCasting)
            {
                Template.SpellTimer.Update(update);

                if (Template.SpellTimer.Elapsed)
                {

                    if (Target == null || Target.CurrentHp == 0 || !Target.WithinRangeOf(this))
                    {

                        var targets = GetObjects<Monster>(Map, i => i.WithinRangeOf(this))
                               .OrderBy(i => i.Position.DistanceFrom(Position));

                        foreach (var t in targets) t.Target = this;

                        var target = Target ?? targets.FirstOrDefault();

                        if (target?.CurrentHp == 0)
                            target = null;

                        if (!CanCast)
                            return;

                        Target = target;
                    }

                    if (Target != null && Target != null && SpellScripts.Count > 0)
                    {
                        var idx = 0;
                        lock (Generator.Random)
                        {
                            idx = Generator.Random.Next(SpellScripts.Count);
                        }

                        SpellScripts[idx].OnUse(this, Target);
                    }

                    Template.SpellTimer.Reset();
                }

            }

            if (Template.AttackTimer != null && Template.EnableWalking)
            {
                Template.AttackTimer.Update(update);
                if (Template.AttackTimer.Elapsed)
                {
                    var targets = GetObjects<Monster>(Map, i => i.WithinRangeOf(this))
                        .OrderBy(i => i.Position.DistanceFrom(Position));

                    foreach (var t in targets) t.Target = this;

                    var target = Target ?? targets.FirstOrDefault();

                    if (target?.CurrentHp == 0)
                        target = null;

                    if (target != null)
                    {
                        Script?.TargetAcquired(target);

                        if (!Position.IsNextTo(target.Position))
                        {
                            WalkTo(target.X, target.Y);
                        }
                        else
                        {
                            if (!Facing(target, out var direction))
                            {
                                Direction = (byte)direction;
                                Turn();
                            }
                            else
                            {
                                target.Target = this;
                                DefaultSkill?.OnUse(this);


                                if (SkillScripts.Count > 0 && target.Target != null)
                                {


                                    var obj = SkillScripts.FirstOrDefault(i => i.Skill.Ready);

                                    if (obj != null)
                                    {
                                        var skill = obj.Skill;

                                        obj?.OnUse(this);
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
                            }
                        }
                    }
                    else
                    {
                        if (Template.PathQualifer.HasFlag(PathQualifer.Patrol))
                        {
                            if (Template.Waypoints == null)
                                Wander();
                            else
                            {
                                if (Template.Waypoints?.Count > 0)
                                    Patrol();
                                else
                                    Wander();
                            }
                        }
                        else
                        {
                            Wander();
                        }
                    }

                    Template.AttackTimer.Reset();
                }
            }
        }

        public void Patrol()
        {
            if (CurrentWaypoint != null)
            {
                WalkTo(CurrentWaypoint.X, CurrentWaypoint.Y);
            }

            if (Position.DistanceFrom(CurrentWaypoint) <= 2 || CurrentWaypoint == null)
            {
                if (WaypointIndex + 1 < Template.Waypoints.Count)
                    WaypointIndex++;
                else
                    WaypointIndex = 0;
            }
        }
    }
}
