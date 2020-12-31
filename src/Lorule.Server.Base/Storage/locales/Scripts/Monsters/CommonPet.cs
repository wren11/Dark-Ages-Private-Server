#region

using System;
using System.Collections.Generic;
using System.Linq;

using Darkages.Network.Game;
using Darkages.Scripting;
using Darkages.Types;

///************************************************************************
//Project Lorule: A Dark Ages Client (http://darkages.creatorlink.net/index/)
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

#endregion

namespace Darkages.Storage.locales.Scripts.Monsters
{
    [Script("Common Pet", "Wren")]
    public class CommonPet : MonsterScript
    {
        private readonly Random _random = new Random();
        private readonly List<SkillScript> _skillScripts = new List<SkillScript>();
        private readonly List<SpellScript> _spellScripts = new List<SpellScript>();

        public CommonPet(Monster monster, Area map)
            : base(monster, map)
        {
            //example lazy load some spell scripts, normally we load these from template.
            LoadSkillScript("Assail", true);
            LoadSpellScript("ard srad");
            LoadSpellScript("ard cradh");
            LoadSpellScript("pramh");
            LoadSpellScript("deo saighead");

            if (Monster.Template.SpellScripts != null)
                foreach (var spellScriptStr in Monster.Template.SpellScripts)
                    LoadSpellScript(spellScriptStr);

            if (Monster.Template.SkillScripts != null)
                foreach (var skillScriptStr in Monster.Template.SkillScripts)
                    LoadSkillScript(skillScriptStr);
        }


        public override void OnDeath(GameClient client)
        {
            client.Aisling.SummonObjects?.Despawn();
        }


        public override void Update(TimeSpan elapsedTime)
        {
            if (Monster == null)
                return;

            if (!Monster.IsAlive)
            {
                Monster.Summoner?.SummonObjects?.Despawn();
                return;
            }

            if (Monster.IsConfused || Monster.IsFrozen || Monster.IsParalyzed || Monster.IsSleeping)
                return;

            if (Monster.Summoner != null)
                if (!Monster.Summoner.View.Contains(Monster))
                {
                    Monster.ShowTo(Monster.Summoner);
                    Monster.Summoner.View.Add(Monster);
                }

            HandleMonsterState(elapsedTime);
        }

        private void HandleMonsterState(TimeSpan elapsedTime)
        {
            #region actions
            void UpdateTarget()
            {
                Monster.Target = GetObjects(Monster.Map, p => p.Target != null && p.Target.Serial == Monster.Summoner?.Serial && p.Target.Serial != Monster.Serial, Get.All)
                    .OrderBy(i => i.Position.DistanceFrom(Monster.Summoner.Position))
                    .FirstOrDefault();

                if (Monster.Target != null)
                {
                    if (Monster.Target.CurrentHp == 0)
                        Monster.Target = null;
                }
            }

            void PetMove()
            {
                if (Monster.WalkTimer.Update(elapsedTime))
                {
                    try
                    {
                        // get target.
                        UpdateTarget();

                        if (Monster.Target == null)
                        {
                            // get the summoner from the obj manager, in case state was lost (summoner re-logged during lifecycle)
                            var summoner = GetObject<Aisling>(null,
                                i => i.Username.ToLower() == Monster.Summoner.Username.ToLower());

                            if (summoner != null && Monster.Position.DistanceFrom(summoner.Position) > 2)
                            {
                                Monster.WalkTo(summoner.X, summoner.Y);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        //ignore
                    }
                }
            }
            void PetCast()
            {
                if (Monster.CastTimer.Update(elapsedTime))
                {
                    if (!Monster.CanCast)
                        return;

                    if (Monster.Target == null)
                        return;

                    if (!Monster.Target.WithinRangeOf(Monster))
                        return;

                    if (Monster?.Target != null && _spellScripts.Count > 0)
                    {
                        var spellidx = _random.Next(_spellScripts.Count);

                        if (_spellScripts[spellidx] != null)
                            _spellScripts[spellidx].OnUse(Monster, Monster.Target);
                    }
                }
            }

            void PetAttack()
            {
                if (!Monster.BashTimer.Update(elapsedTime)) return;

                if (Monster.Target != null && (!Monster.WithinRangeOf(Monster.Target) || Monster.Position.DistanceFrom(Monster.Target.Position) <= 0)) return;

                if (Monster.Target != null && !Monster.WalkTo(Monster.Target.X, Monster.Target.Y))
                {

                } 
                
                if (Monster.Target != null && !Monster.Facing(Monster.Target.X, Monster.Target.Y, out var facingDirection))
                {
                    Monster.Direction = (byte) facingDirection;
                    Monster.Turn();
                    {
                        DefaultAttack();
                    }
                }
                else
                {
                    if (Monster.Target == null)
                        return;

                    DefaultAttack();
                }
            }

            bool DefaultAttack()
            {
                var sObj = _skillScripts.FirstOrDefault(i => i.Skill.Ready);

                if (sObj == null)
                    return true;

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
                return false;
            }
            #endregion

            PetAttack();
            PetMove();
            PetCast();
        }


        #region Scripts
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
        #endregion

        #region unused
        public override void OnApproach(GameClient client)
        {

        }

        public override void OnAttacked(GameClient client)
        {

        }

        public override void OnCast(GameClient client)
        {

        }

        public override void OnClick(GameClient client)
        {

        }

        public override void OnDamaged(GameClient client, int dmg, Sprite source)
        {

        }

        public override void OnLeave(GameClient client)
        {

        }

        public override void OnSkulled(GameClient client)
        {

        }
        #endregion
    }
}