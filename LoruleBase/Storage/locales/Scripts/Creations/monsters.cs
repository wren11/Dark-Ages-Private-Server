using System;
using System.Collections.Generic;
using System.Linq;
using Darkages.Common;
using Darkages.Network.Game;
using Darkages.Scripting;
using Darkages.Systems.Loot;
using Darkages.Types;
using static Darkages.Common.Generator;

namespace Darkages.Storage.locales.Scripts.Creations
{
    [Script("Create Monster", "Wren", "Default Monster Creation Script")]
    public class Monsters : MonsterCreateScript
    {
        private readonly MonsterTemplate _template;

        public Monsters(MonsterTemplate template)
        {
            _template = template;
        }

        public override Monster Create(MonsterTemplate template, Area map)
        {
            void TemplateCreationSanityChecks()
            {
                if (template.CastSpeed == 0)
                    template.CastSpeed = 2000;

                if (template.AttackSpeed == 0)
                    template.AttackSpeed = 1000;

                if (template.MovementSpeed == 0)
                    template.MovementSpeed = 2000;

                if (template.Level <= 0)
                    template.Level = 1;
            }

            bool FindBestMonsterMapSlot(Monster monster)
            {
                switch (template.SpawnType)
                {
                    case SpawnQualifer.Random:
                        {
                            var x = Generator.Random.Next(1, map.Cols);
                            var y = Generator.Random.Next(1, map.Rows);

                            monster.XPos = x;
                            monster.YPos = y;

                            if (map.IsWall(x, y))
                                return true;
                            break;
                        }
                    case SpawnQualifer.Defined:
                        monster.XPos = template.DefinedX;
                        monster.YPos = template.DefinedY;
                        break;
                }

                return false;
            }

            //validate values in template.
            TemplateCreationSanityChecks();

            var obj = new Monster
            {
                Template = template,
                CastTimer = new GameServerTimer(TimeSpan.FromMilliseconds(1 + template.CastSpeed)),
                BashTimer = new GameServerTimer(TimeSpan.FromMilliseconds(1 + template.AttackSpeed)),
                WalkTimer = new GameServerTimer(TimeSpan.FromMilliseconds(1 + template.MovementSpeed)),
                CastEnabled = template.MaximumMP > 0,
                TaggedAislings = new HashSet<int>()
            };

            if (obj.Template.Grow)
                obj.Template.Level++;

            var mod = (obj.Template.Level + 1) * 0.01;
            var hp = mod + 50 + obj.Template.Level * (obj.Template.Level + 40);
            var mp = hp / 3;

            obj.Template.MaximumHP = (int)hp;
            obj.Template.MaximumMP = (int)mp;

            var stat = RandomEnumValue<PrimaryStat>();

            obj._Str = 1;
            obj._Int = 1;
            obj._Wis = 1;
            obj._Con = 1;
            obj._Dex = 1;

            switch (stat)
            {
                case PrimaryStat.STR:
                    obj._Str += (byte)(obj.Template.Level * 0.5 * 2);
                    break;

                case PrimaryStat.INT:
                    obj._Int += (byte)(obj.Template.Level * 0.5 * 2);
                    break;

                case PrimaryStat.WIS:
                    obj._Wis += (byte)(obj.Template.Level * 0.5 * 2);
                    break;

                case PrimaryStat.CON:
                    obj._Con += (byte)(obj.Template.Level * 0.5 * 2);
                    break;

                case PrimaryStat.DEX:
                    obj._Dex += (byte)(obj.Template.Level * 0.5 * 2);
                    break;
            }

            obj.MajorAttribute = stat;

            obj.BonusAc = (int)(70 - obj.Template.Level * 0.5 / 1.0);

            if (obj.BonusAc < -70) obj.BonusAc = -70;

            obj.DefenseElement = ElementManager.Element.None;
            obj.OffenseElement = ElementManager.Element.None;

            if (obj.Template.ElementType == ElementQualifer.Random)
            {
                obj.DefenseElement = RandomEnumValue<ElementManager.Element>();
                obj.OffenseElement = RandomEnumValue<ElementManager.Element>();
            }
            else if (obj.Template.ElementType == ElementQualifer.Defined)
            {
                obj.DefenseElement = template.DefenseElement == ElementManager.Element.None
                    ? RandomEnumValue<ElementManager.Element>()
                    : template.DefenseElement;
                obj.OffenseElement = template.OffenseElement == ElementManager.Element.None
                    ? RandomEnumValue<ElementManager.Element>()
                    : template.OffenseElement;
            }

            obj.BonusMr = (byte)(10 * (template.Level / 20));

            if (obj.BonusMr > ServerContext.Config.BaseMR)
                obj.BonusMr = ServerContext.Config.BaseMR;

            if ((template.PathQualifer & PathQualifer.Wander) == PathQualifer.Wander)
                obj.WalkEnabled = true;
            else if ((template.PathQualifer & PathQualifer.Fixed) == PathQualifer.Fixed)
                obj.WalkEnabled = false;
            else if ((template.PathQualifer & PathQualifer.Patrol) == PathQualifer.Patrol)
                obj.WalkEnabled = true;

            if (template.MoodType.HasFlag(MoodQualifer.Aggressive))
                obj.Aggressive = true;
            else if (template.MoodType.HasFlag(MoodQualifer.Unpredicable))
                lock (Generator.Random)
                {
                    obj.Aggressive = Generator.Random.Next(1, 101) > 50;
                }
            else
                obj.Aggressive = false;

            //Based on Spawn type, Work out where i need to spawn.
            if (FindBestMonsterMapSlot(obj))
                return null;

            lock (Generator.Random)
            {
                obj.Serial = GenerateNumber();
            }

            obj.CurrentMapId = map.ID;
            obj.CurrentHp = template.MaximumHP;
            obj.CurrentMp = template.MaximumMP;
            obj._MaximumHp = template.MaximumHP;
            obj._MaximumMp = template.MaximumMP;
            obj.AbandonedDate = DateTime.UtcNow;

            lock (Generator.Random)
            {
                obj.Image = template.ImageVarience
                            > 0
                    ? (ushort)Generator.Random.Next(template.Image, template.Image + template.ImageVarience)
                    : template.Image;
            }

            //Don't load scripts on creation. Instead on Approach.
            //InitScripting(template, map, obj);

            if (!obj.Template.LootType.HasFlag(LootQualifer.Table)) return obj;
            obj.LootManager = new LootDropper();
            obj.LootTable = new LootTable(template.Name);
            obj.UpgradeTable = new LootTable("Probabilities");

            foreach (var drop in obj.Template.Drops)
                if (drop.Equals("random", StringComparison.OrdinalIgnoreCase))
                {
                    lock (Generator.Random)
                    {
                        var available = ServerContext.GlobalItemTemplateCache.Select(i => i.Value)
                            .Where(i => Math.Abs(i.LevelRequired - obj.Template.Level) <= 10).ToList();
                        if (available.Count > 0) obj.LootTable.Add(available[GenerateNumber() % available.Count]);
                    }
                }
                else
                {
                    if (ServerContext.GlobalItemTemplateCache.ContainsKey(drop))
                        obj.LootTable.Add(ServerContext.GlobalItemTemplateCache[drop]);
                }

            obj.UpgradeTable.Add(new Types.Common());
            obj.UpgradeTable.Add(new Uncommon());
            obj.UpgradeTable.Add(new Rare());
            obj.UpgradeTable.Add(new Epic());
            obj.UpgradeTable.Add(new Legendary());
            obj.UpgradeTable.Add(new Mythical());
            obj.UpgradeTable.Add(new Godly());
            obj.UpgradeTable.Add(new Forsaken());

            return obj;
        }
    }
}
