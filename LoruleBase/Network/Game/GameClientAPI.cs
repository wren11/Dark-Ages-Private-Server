#region

using System;
using System.Linq;
using System.Threading.Tasks;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;

#endregion

namespace Darkages.Network.Game
{
    public partial class GameClient : IGameClient
    {
        public GameClient GhostFormToAisling()
        {
            Aisling.Flags = AislingFlags.Normal;
            HpRegenTimer.Disabled = false;
            MpRegenTimer.Disabled = false;

            Refresh(true);
            return this;
        }

        public GameClient LearnSkill(Mundane source, SkillTemplate subject, string message)
        {
            var canLearn = false;

            if (subject.Prerequisites != null)
                canLearn = PayPrerequisites(subject.Prerequisites);

            if (subject.LearningRequirements != null && subject.LearningRequirements.Any())
                canLearn = subject.LearningRequirements.TrueForAll(PayPrerequisites);

            if (!canLearn)
                return this;

            Skill.GiveTo(this, subject.Name);
            SendOptionsDialog(source, message);

            Aisling.Show(Scope.NearbyAislings,
                new ServerFormat29((uint)Aisling.Serial, (uint)source.Serial,
                    subject.TargetAnimation,
                    subject.TargetAnimation, 100));

            return this;
        }

        public GameClient LearnSpell(Mundane source, SpellTemplate subject, string message)
        {
            var canLearn = false;

            if (subject.Prerequisites != null) canLearn = PayPrerequisites(subject.Prerequisites);

            if (subject.LearningRequirements != null && subject.LearningRequirements.Any())
                canLearn = subject.LearningRequirements.TrueForAll(PayPrerequisites);

            if (!canLearn)
                return this;

            Spell.GiveTo(this, subject.Name);
            SendOptionsDialog(source, message);

            Aisling.Show(Scope.NearbyAislings,
                new ServerFormat29((uint)Aisling.Serial, (uint)source.Serial,
                    subject.TargetAnimation,
                    subject.TargetAnimation, 100));

            return this;
        }

        public bool CastSpell(string spellName, Sprite caster, Sprite target)
        {
            if (ServerContext.GlobalSpellTemplateCache.ContainsKey(spellName))
            {
                var scripts = ScriptManager.Load<SpellScript>(spellName,
                    Spell.Create(1, ServerContext.GlobalSpellTemplateCache[spellName]));
                {
                    foreach (var script in scripts.Values) script.OnUse(caster, target);

                    return true;
                }
            }

            return false;
        }

        public async Task Effect(ushort n, int d = 1000, int r = 1)
        {
            if (r <= 0)
                r = 1;

            for (var i = 0; i < r; i++)
            {
                Aisling.SendAnimation(n, Aisling, Aisling);

                foreach (var obj in Aisling.MonstersNearby()) obj.SendAnimation(n, obj.Position);
                await Task.Delay(d);
            }
        }

        public void ForgetSkill(string s)
        {
            var subject = Aisling.SkillBook.Skills.Values
                .FirstOrDefault(i => i?.Template != null && !string.IsNullOrEmpty(i.Template.Name) && i.Template.Name.ToLower() == s.ToLower());

            if (subject != null)
            {
                Aisling.SkillBook.Remove(subject.Slot);
                {
                    Send(new ServerFormat2D(subject.Slot));
                }
            }

            LoadSkillBook();
        }

        public void ForgetSpell(string s)
        {
            var subject = Aisling.SpellBook.Spells.Values
                .FirstOrDefault(i => i?.Template != null && !string.IsNullOrEmpty(i.Template.Name) && i.Template.Name.ToLower() == s.ToLower());

            if (subject != null)
            {
                Aisling.SpellBook.Remove(subject.Slot);
                {
                    Send(new ServerFormat18(subject.Slot));
                }
            }

            LoadSpellBook();
        }

        public void GiveCon(byte v = 1)
        {
            Aisling._Wis += v;

            if (Aisling._Wis > ServerContext.Config.StatCap)
                Aisling._Wis = ServerContext.Config.StatCap;

            SendStats(StatusFlags.All);
        }

        public void GiveDex(byte v = 1)
        {
            Aisling._Dex += v;

            if (Aisling._Dex > ServerContext.Config.StatCap)
                Aisling._Dex = ServerContext.Config.StatCap;

            SendStats(StatusFlags.All);
        }

        public void GiveExp(int a)
        {
            Monster.DistributeExperience(Aisling, a);
        }

        public void GiveHp(int v = 1)
        {
            Aisling._MaximumHp += v;

            if (Aisling._MaximumHp > ServerContext.Config.MaxHP)
                Aisling._MaximumHp = ServerContext.Config.MaxHP;

            SendStats(StatusFlags.All);
        }

        public void GiveInt(byte v = 1)
        {
            Aisling._Int += v;

            if (Aisling._Int > ServerContext.Config.StatCap)
                Aisling._Int = ServerContext.Config.StatCap;

            SendStats(StatusFlags.All);
        }

        public bool GiveItem(string itemName)
        {
            var item = Item.Create(Aisling, itemName);

            if (item != null) return item.GiveTo(Aisling);

            return false;
        }

        public void GiveMp(int v = 1)
        {
            Aisling._MaximumMp += v;

            if (Aisling._MaximumMp > ServerContext.Config.MaxHP)
                Aisling._MaximumMp = ServerContext.Config.MaxHP;

            SendStats(StatusFlags.All);
        }

        public void GiveScar()
        {
            Aisling.LegendBook.AddLegend(new Legend.LegendItem
            {
                Category = "Event",
                Color = (byte)LegendColor.LightOrange,
                Icon = (byte)LegendIcon.Rogue,
                Value = "Scar of Sgrios"
            });
        }

        public void GiveStr(byte v = 1)
        {
            Aisling._Str += v;

            if (Aisling._Str > ServerContext.Config.StatCap)
                Aisling._Str = ServerContext.Config.StatCap;

            SendStats(StatusFlags.All);
        }

        public bool GiveTutorialArmor()
        {
            var item = Aisling.Gender == Gender.Male ? "Shirt" : "Blouse";
            return GiveItem(item);
        }

        public void GiveWis(byte v = 1)
        {
            Aisling._Wis += v;

            if (Aisling._Wis > ServerContext.Config.StatCap)
                Aisling._Wis = ServerContext.Config.StatCap;

            SendStats(StatusFlags.All);
        }

        public bool IsBehind(Sprite sprite)
        {
            var delta = sprite.Direction - Aisling.Direction;
            return Aisling.Position.IsNextTo(sprite.Position) && delta == -1;
        }

        public void KillPlayer(string u)
        {
            var user = GetObject<Aisling>(null, i => i.Username.Equals(u, StringComparison.OrdinalIgnoreCase));

            if (user != null)
                user.CurrentHp = 0;
        }

        public void LearnEverything()
        {
            foreach (var skill in ServerContext.GlobalSkillTemplateCache.Values)
            {
                if (skill != null)
                {
                    ForgetSkill(skill.Name);
                    Skill.GiveTo(Aisling, skill.Name);
                }
            }

            foreach (var spell in ServerContext.GlobalSpellTemplateCache.Values)
            {
                if (spell != null)
                {
                    ForgetSpell(spell.Name);
                    Spell.GiveTo(Aisling, spell.Name);
                }
            }

            foreach (var skill in ServerContext.GlobalSkillTemplateCache.Values)
            {
                if (skill != null) Skill.GiveTo(Aisling, skill.Name);
            }

            foreach (var spell in ServerContext.GlobalSpellTemplateCache.Values)
            {
                if (spell != null) Spell.GiveTo(Aisling, spell.Name);
            }


            LoadSkillBook();
            LoadSpellBook();
        }

        public GameClient LoggedIn(bool state)
        {
            Aisling.LoggedIn = state;

            return this;
        }

        public void OpenBoard(string n)
        {
            if (ServerContext.GlobalBoardCache.ContainsKey(n))
            {
                var boardListObj = ServerContext.GlobalBoardCache[n];

                if (boardListObj != null && boardListObj.Any())
                    Send(new BoardList(boardListObj));
            }
        }

        public bool PlayerUseSkill(string spellname)
        {
            var skill = Aisling.SkillBook
                .Get(i => i.Template.Name.Equals(spellname, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (skill != null)
            {
                if (skill.Scripts?.Values != null)
                    foreach (var script in skill.Scripts?.Values)
                        script.OnUse(Aisling);
                return true;
            }

            return false;
        }

        public bool PlayerUseSpell(string spellname, Sprite target)
        {
            var spell = Aisling.SpellBook
                .Get(i => i.Template.Name.Equals(spellname, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (spell != null)
            {
                foreach (var script in spell.Scripts.Values)
                    script?.OnUse(Aisling, target);

                return true;
            }

            return false;
        }

        public void Port(int i, int x = 0, int y = 0)
        {
            TransitionToMap(i, new Position(x, y));

            SystemMessage("Port: Success.");
        }

        public void Recover()
        {
            Revive();
        }

        public void ReloadObjects(bool all = false)
        {
            var objs = GetObjects(null, i => i != null && i.Serial != Aisling.Serial,
                all ? Get.All : Get.Items | Get.Money | Get.Monsters | Get.Mundanes);

            foreach (var obj in objs) obj.Remove();

            ServerContext.LoadAndCacheStorage();
        }

        public void RevivePlayer(string u)
        {
            var user = GetObject<Aisling>(null, i => i.Username.Equals(u, StringComparison.OrdinalIgnoreCase));

            if (user != null && user.LoggedIn)
                user.Client.Revive();
        }

        public void Spawn(string t, int x, int y, int c)
        {
            var name = t.Replace("-", string.Empty).Trim();

            var obj = ServerContext.GlobalMonsterTemplateCache
                .FirstOrDefault(i => i.Name.Equals(name, StringComparison.CurrentCulture));

            if (obj != null)
            {
                for (var i = 0; i < c; i++)
                {
                    var mon = Monster.Create(obj, Aisling.Map);
                    if (mon != null)
                    {
                        mon.XPos = x;
                        mon.YPos = y;

                        AddObject(mon);
                    }
                }

                SystemMessage("spawnMonster: Success.");
            }
            else
            {
                SystemMessage("spawnMonster: Failed.");
            }
        }

        public void StressTest()
        {
            Task.Run(async () =>
            {
                for (var n = 0; n < 5000; n++)
                    for (byte i = 0; i < 100; i++)
                        await Effect(i, 500);
            });
        }

        public bool TakeAwayItem(string item)
        {
            var itemObj = Aisling.Inventory.Has(i => i.Template.Name.Equals(item, StringComparison.OrdinalIgnoreCase));

            if (itemObj != null)
            {
                Aisling.Inventory.Remove(this, itemObj);
                return true;
            }

            return false;
        }

        public bool TakeAwayItem(Item item)
        {
            if (item != null)
            {
                Aisling.Inventory.Remove(this, item);
                return true;
            }

            return false;
        }
    }
}