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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Darkages.Common;
using Darkages.Network;
using Darkages.Network.Game;
using Darkages.Network.Object;
using Darkages.Network.ServerFormats;
using LiteDB;
using Newtonsoft.Json;
using static Darkages.Types.ElementManager;

namespace Darkages.Types
{
    public abstract class Sprite : ObjectManager, INotifyPropertyChanged
    {
        private readonly Random _rnd = new Random();

        [JsonIgnore]
        [BsonIgnore]
        private Random rnd
        {
            get => _rnd;
        }


        [JsonIgnore] [BsonIgnore] public byte LastDirection;

        [JsonIgnore] [BsonIgnore] public Position LastPosition;


        #region Sprite Constructor

        public Sprite()
        {
            if (this is Aisling)
                EntityType = TileContent.Aisling;
            if (this is Monster)
                EntityType = TileContent.Monster;
            if (this is Mundane)
                EntityType = TileContent.Mundane;
            if (this is Money)
                EntityType = TileContent.None;
            if (this is Item)
                EntityType = TileContent.None;

            Amplified = 0;
            Target = null;


            Buffs = new ConcurrentDictionary<string, Buff>();
            Debuffs = new ConcurrentDictionary<string, Debuff>();

            LastTargetAcquired = DateTime.UtcNow;
            LastMovementChanged = DateTime.UtcNow;
            LastUpdated = DateTime.UtcNow;
            LastPosition = new Position(0, 0);
            LastDirection = 0;
        }

        #endregion


        [JsonIgnore] [BsonIgnore] public GameClient Client { get; set; }

        [JsonIgnore] [BsonIgnore]
        public Area Map => ServerContext.GlobalMapCache.ContainsKey(CurrentMapId)
            ? ServerContext.GlobalMapCache[CurrentMapId] ?? null
            : null;


        [JsonIgnore] [BsonIgnore] public TileContent EntityType { get; set; }

        [JsonIgnore] [BsonIgnore] public Sprite Target { get; set; }

        [JsonIgnore] [BsonIgnore] public Position Position => new Position(XPos, YPos);

        [JsonIgnore] [BsonIgnore] public bool Attackable => this is Monster || this is Aisling || this is Mundane;

        [JsonIgnore] [BsonIgnore] public bool Alive => CurrentHp > 0;

        [JsonIgnore] [BsonIgnore] public DateTime AbandonedDate { get; set; }

        [JsonIgnore] [BsonIgnore] public DateTime LastUpdated { get; set; }

        [JsonIgnore] [BsonIgnore] public DateTime LastTargetAcquired { get; set; }

        [JsonIgnore] [BsonIgnore] public DateTime LastMovementChanged { get; set; }

        [JsonIgnore] [BsonIgnore]
        public int Level => EntityType == TileContent.Aisling ? (this as Aisling).ExpLevel
            : EntityType == TileContent.Monster ? (this as Monster).Template.Level
            : EntityType == TileContent.Mundane ? (this as Mundane).Template.Level
            : EntityType == TileContent.Item ? (this as Item).Template.LevelRequired : 0;

        public ConcurrentDictionary<string, Debuff> Debuffs { get; set; }

        public ConcurrentDictionary<string, Buff> Buffs { get; set; }

       private object syncLock = new object();

        public Element OffenseElement { get; set; }

        public Element DefenseElement { get; set; }

        public PrimaryStat MajorAttribute { get; set; }

        public byte Direction { get; set; }

        public int CurrentMapId { get; set; }

        public int Amplified { get; set; }


        [JsonIgnore] [BsonIgnore] public bool CanMove => !(IsFrozen || IsSleeping || IsParalyzed);

        [JsonIgnore] [BsonIgnore] public bool CanCast => !(IsFrozen || IsSleeping);

        [JsonIgnore] [BsonIgnore] public bool EmpoweredAssail { get; set; }

        public bool Immunity { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Identification & Position

        [BsonId]
        public int Serial { get; set; }

        public int X;

        public int Y;

        [JsonIgnore] [BsonIgnore]
        public int XPos
        {
            get => X;
            set
            {
                if (X != value)
                {
                    X = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [JsonIgnore] [BsonIgnore]
        public int YPos
        {
            get => Y;
            set
            {
                if (Y != value)
                {
                    Y = value;
                    NotifyPropertyChanged();
                }
            }
        }

        #endregion

        #region Attributes

        public int CurrentHp { get; set; }

        public int CurrentMp { get; set; }

        public int _MaximumHp { get; set; }

        public int _MaximumMp { get; set; }

        public byte _Str { get; set; }

        public byte _Int { get; set; }

        public byte _Wis { get; set; }

        public byte _Con { get; set; }

        public byte _Dex { get; set; }

        public byte _Mr { get; set; }

        public byte _Dmg { get; set; }

        public byte _Hit { get; set; }

        public int _Regen { get; set; }

        [JsonIgnore] [BsonIgnore] public int Regen => (_Regen + BonusRegen).Clamp(0, 300);

        [JsonIgnore] [BsonIgnore] public int MaximumHp => _MaximumHp + BonusHp;

        [JsonIgnore] [BsonIgnore] public int MaximumMp => _MaximumMp + BonusMp;

        [JsonIgnore] [BsonIgnore]
        public byte Str
        {
            get
            {
                var tmp = (byte) (_Str + BonusStr).Clamp(1, byte.MaxValue);
                if (tmp > 255)
                    return 255;

                return tmp;
            }
        }

        [JsonIgnore] [BsonIgnore]
        public byte Int
        {
            get
            {
                var tmp = (byte) (_Int + BonusInt).Clamp(1, byte.MaxValue);
                if (tmp > 255)
                    return 255;

                return tmp;
            }
        }

        [JsonIgnore] [BsonIgnore]
        public byte Wis
        {
            get
            {
                var tmp = (byte) (_Wis + BonusWis).Clamp(1, byte.MaxValue);
                if (tmp > 255)
                    return 255;

                return tmp;
            }
        }

        [JsonIgnore] [BsonIgnore]
        public byte Con
        {
            get
            {
                var tmp = (byte) (_Con + BonusCon).Clamp(1, byte.MaxValue);
                if (tmp > 255)
                    return 255;

                return tmp;
            }
        }

        [JsonIgnore] [BsonIgnore]
        public byte Dex
        {
            get
            {
                var tmp = (byte) (_Dex + BonusDex).Clamp(1, byte.MaxValue);
                if (tmp > 255)
                    return 255;

                return tmp;
            }
        }

        [JsonIgnore] [BsonIgnore]
        public int Ac
        {
            get
            {
                if (BonusAc < -70)
                    return -70;

                return BonusAc;
            }
        }


        [JsonIgnore] [BsonIgnore] public byte Mr => (byte) (_Mr + BonusMr).Clamp(0, 70);

        [JsonIgnore] [BsonIgnore] public byte Dmg => (byte) (_Dmg + BonusDmg).Clamp(0, byte.MaxValue);

        [JsonIgnore] [BsonIgnore] public byte Hit => (byte) (_Hit + BonusHit).Clamp(0, byte.MaxValue);

        [JsonIgnore] [BsonIgnore] public int BonusStr { get; set; }

        [JsonIgnore] [BsonIgnore] public int BonusInt { get; set; }

        [JsonIgnore] [BsonIgnore] public int BonusWis { get; set; }

        [JsonIgnore] [BsonIgnore] public int BonusCon { get; set; }

        [JsonIgnore] [BsonIgnore] public int BonusDex { get; set; }

        [JsonIgnore] [BsonIgnore] public byte BonusMr { get; set; }

        [JsonIgnore] [BsonIgnore] public int BonusAc { get; set; }

        [JsonIgnore] [BsonIgnore] public byte BonusHit { get; set; }

        [JsonIgnore] [BsonIgnore] public byte BonusDmg { get; set; }

        [JsonIgnore] [BsonIgnore] public int BonusHp { get; set; }

        [JsonIgnore] [BsonIgnore] public int BonusMp { get; set; }

        [JsonIgnore] [BsonIgnore] public int BonusRegen { get; set; }

        #endregion

        #region Status

        [JsonIgnore] [BsonIgnore] public bool IsAited => HasBuff("aite");

        [JsonIgnore] [BsonIgnore] public bool IsSleeping => HasDebuff("sleep");

        [JsonIgnore] [BsonIgnore] public bool IsFrozen => HasDebuff("frozen");

        [JsonIgnore] [BsonIgnore] public bool IsPoisoned => HasDebuff(i => i.Name.ToLower().Contains("puinsein"));

        [JsonIgnore] [BsonIgnore] public bool IsCursed => HasDebuff(i => i.Name.ToLower().Contains("cradh"));

        [JsonIgnore] [BsonIgnore] public bool IsBleeding => HasDebuff("bleeding");

        [JsonIgnore] [BsonIgnore] public bool IsBlind => HasDebuff("blind");

        [JsonIgnore] [BsonIgnore] public bool IsConfused => HasDebuff("confused");

        [JsonIgnore] [BsonIgnore]
        public bool IsParalyzed => HasDebuff("paralyze") || HasDebuff(i => i.Name.ToLower().Contains("beag suain"));

        [JsonIgnore] [BsonIgnore]
        public int[][] Directions { get; } =
        {
            new[] {+0, -1},
            new[] {+1, +0},
            new[] {+0, +1},
            new[] {-1, +0}
        };

        [JsonIgnore] [BsonIgnore]
        public int[][] DirectionTable { get; } =
        {
            new[] {-1, +3, -1},
            new[] {+0, -1, +2},
            new[] {-1, +1, -1}
        };

        [JsonIgnore] [BsonIgnore] public bool Exists => GetObject(Map, i => i.Serial == Serial, Get.All) != null;

        /// <summary>
        /// New Buff Property, Let's persist a value to track our reflect spell condition.
        /// </summary>
        public bool SpellReflect { get; set; }

        #endregion

        public TSprite Cast<TSprite>()
            where TSprite : Sprite
        {
            return this as TSprite;
        }

        #region Sprite Methods

        public bool TrapsAreNearby()
        {
            return Trap.Traps.Select(i => i.Value).Any(i => i.CurrentMapId == CurrentMapId);
        }

        public bool TriggerNearbyTraps()
        {
            var trap = Trap.Traps.Select(i => i.Value).FirstOrDefault(i => i.Owner.Serial != Serial && i.CurrentMapId == CurrentMapId);

            if (trap != null)
            {
                if (X == trap.Location.X && Y == trap.Location.Y)
                {
                    Trap.Activate(trap, this);
                }
            }
            return false;
        }

        public bool HasBuff(string buff)
        {
            if (Buffs == null || Buffs.Count == 0)
                return false;

            return Buffs.ContainsKey(buff);
        }

        public bool HasDebuff(string debuff)
        {
            if (Debuffs == null || Debuffs.Count == 0)
                return false;

            return Debuffs.ContainsKey(debuff);
        }

        public bool HasDebuff(Func<Debuff, bool> p)
        {
            if (Debuffs == null || Debuffs.Count == 0)
                return false;

            return Debuffs.Select(i => i.Value).FirstOrDefault(p) != null;
        }

        public string GetDebuffName(Func<Debuff, bool> p)
        {
            if (Debuffs == null || Debuffs.Count == 0)
                return string.Empty;

            return Debuffs.Select(i => i.Value)
                .FirstOrDefault(p)
                ?.Name;
        }

        public bool RemoveBuff(string buff)
        {
            if (HasBuff(buff))
            {
                var buffobj = Buffs[buff];
                buffobj?.OnEnded(this, buffobj);

                return true;
            }

            return false;
        }

        public bool RemoveDebuff(string debuff, bool cancelled = false)
        {
            if (!cancelled && debuff == "skulled")
                return true;

            if (HasDebuff(debuff))
            {
                var buffobj = Debuffs[debuff];

                if (buffobj != null)
                {
                    buffobj.Cancelled = cancelled;
                    buffobj.OnEnded(this, buffobj);
                    return true;
                }
            }

            return false;
        }

        public int GetBaseDamage(Sprite target, MonsterDamageType type)
        {
            if (this is Monster || this is Mundane)
            {
                var mod  = 0.0;
                var diff = 0;

                if (target is Aisling obj)
                    diff = Level + 1 - obj.ExpLevel;

                if (target is Monster tmon)
                    diff = Level + 1 - tmon.Template.Level;

                if (diff <= 0)
                    mod = Level * (type == MonsterDamageType.Physical ? 0.1 : 2) * 60;
                else
                    mod = Level * (type == MonsterDamageType.Physical ? 0.1 : 2) * (60 * diff);


                var dmg = Math.Abs((int) (mod + 1));

                if (dmg <= 0)
                    dmg = 1;

                return dmg;
            }

            return 1;
        }

        public void RemoveAllBuffs()
        {
            if (Buffs != null)
                foreach (var buff in Buffs)
                    RemoveBuff(buff.Key);
        }

        public void RemoveAllDebuffs()
        {
            if (Debuffs != null)
                foreach (var debuff in Debuffs)
                    RemoveDebuff(debuff.Key);
        }

        public void RemoveBuffsAndDebuffs()
        {
            RemoveAllBuffs();
            RemoveAllDebuffs();
        }

        public void ApplyDamage(Sprite source, int dmg, Element element, byte sound = 1)
        {
            element = CheckRandomElement(element);

            var saved = source.OffenseElement;
            {
                source.OffenseElement = element;
                ApplyDamage(source, dmg, false, sound);
                source.OffenseElement = saved;
            }
        }

        public static Element CheckRandomElement(Element element)
        {
            if (element == Element.Random)
                element = Generator.RandomEnumValue<Element>();

            return element;
        }

        public bool CanTag(Aisling AttackingPlayer, bool force = false)
        {
            bool canTag = false;

            if (!(this is Monster monster))
                return false;

            if (monster.TaggedAislings.Any(i => i == AttackingPlayer.Serial))
                canTag = true;

            if (monster.TaggedAislings.Count == 0)
                canTag = true;

            List<int> tagstoRemove = new List<int>();
            foreach (var userId in monster.TaggedAislings.Where(i => i != AttackingPlayer.Serial))
            {
                var taggeduser = GetObject<Aisling>(Map, i => i.Serial == userId);

                if (taggeduser != null)
                {
                    if (taggeduser.WithinRangeOf(this))
                    {
                        canTag = AttackingPlayer.GroupParty.Has(taggeduser);
                    }
                    else
                    {
                        tagstoRemove.Add(taggeduser.Serial);
                        canTag = true;
                    }
                }
            }

            var lostTags = monster.AislingsNearby().Where(i => monster.TaggedAislings.Contains(i.Serial));

            if (!lostTags.Any())
            {
                canTag = true;
            }
            

            monster.TaggedAislings.RemoveWhere(n => tagstoRemove.Contains(n));


            if (canTag)
            {
                monster.AppendTags(AttackingPlayer);

                if (monster.Target == null)
                    monster.Target = AttackingPlayer;
            }

            if (force)
            {
                canTag = false;
            }

            return canTag;
        }
        
        public void ApplyDamage(Sprite damageDealingSprite, int dmg,  bool penetrating = false, byte sound = 1, Action<int> dmgcb = null, bool forceTarget = false)
        {
            if (!WithinRangeOf(damageDealingSprite))
                return;

            if (!Attackable)
                return;

            if (!CanBeAttackedHere(damageDealingSprite))
                return;
            
            if (dmg == -1)
            {
                dmg         = CurrentHp;
                penetrating = true;
            }

            dmg = ApplyWeaponBonuses(damageDealingSprite, dmg);

            if (dmg > 0)
                ApplyEquipmentDurability(dmg);


            if (!DamageTarget(damageDealingSprite, ref dmg, penetrating, sound, dmgcb, forceTarget))
                return;

            OnDamaged(damageDealingSprite, dmg);
        }

        private void OnDamaged(Sprite source, int dmg)
        {
            (this as Aisling)?.Client.SendStats(StatusFlags.StructB);
            (source as Aisling)?.Client.SendStats(StatusFlags.StructB);

            if (!(this is Monster))
                return;

            if (source is Aisling aisling)
            {
                (this as Monster)?.Script?.OnDamaged(aisling?.Client, dmg, source);
            }
        }

        private bool DamageTarget(Sprite damageDealingSprite, ref int dmg, bool penetrating, byte sound, Action<int> dmgcb, bool forced)
        {
            if (penetrating)
            {
                var empty = new ServerFormat13
                {
                    Serial = Serial,
                    Health = byte.MaxValue,
                    Sound  = sound
                };

                Show(Scope.VeryNearbyAislings, empty);

                CurrentHp -= dmg;

                if (CurrentHp < 0)
                    CurrentHp = 0;

                return true;
            }
            else
            {
                if (damageDealingSprite is Aisling _aisling)
                {
                    if (!CanTag(_aisling, forced))
                    {
                        _aisling.Client.SendMessage(0x02, ServerContext.Config.CantAttack);
                        return false;
                    }
                }

                if (Immunity)
                {
                    var empty = new ServerFormat13
                    {
                        Serial = Serial,
                        Health = byte.MaxValue,
                        Sound  = sound
                    };

                    Show(Scope.VeryNearbyAislings, empty);
                    return false;
                }

                if (HasDebuff("sleep"))
                    dmg <<= 1;

                RemoveDebuff("sleep");

                if (IsAited && dmg > 5)
                    dmg /= 3;

                var amplifier = GetElementalModifier(damageDealingSprite);
                {
                    dmg = ComputeDmgFromAc(dmg);
                    dmg = CompleteDamageApplication(dmg, sound, dmgcb, amplifier);
                }
            }

            return true;
        }

        private int ApplyWeaponBonuses(Sprite source, int dmg)
        {
            if (source is Aisling aisling)
            {
                if (aisling.EquipmentManager.Weapon?.Item != null && aisling.Weapon > 0)
                {
                    var weapon = aisling.EquipmentManager.Weapon.Item;

                    lock (rnd)
                    {
                        dmg += rnd.Next(
                                   weapon.Template.DmgMin + 1, 
                                   weapon.Template.DmgMax + 5) + aisling.BonusDmg * 10 / 100;
                    }
                }
            }

            return dmg;
        }

        private void ApplyEquipmentDurability(int dmg)
        {
            if (this is Aisling aisling && aisling.DamageCounter++ % 2 == 0 && dmg > 0)
                aisling.EquipmentManager.DecreaseDurability();
        }

        private double GetElementalModifier(Sprite damageDealingSprite)
        {
            if (damageDealingSprite == null)
                return 1;


            var element = CheckRandomElement(damageDealingSprite.OffenseElement);
            var saved = DefenseElement;
            var amplifier = CalcaluteElementalAmplifier(element);
            {
                DefenseElement = saved;
            }

            if (Amplified > 0)
            {
                amplifier *= Amplified == 1 ? ServerContext.Config.FasNadurStrength + 10
                    : ServerContext.Config.MorFasNadurStrength + 30;
            }

            return amplifier;
        }
        
        private double CalcaluteElementalAmplifier(Element element)
        {
            while (DefenseElement == Element.Random)
            {
                DefenseElement = CheckRandomElement(DefenseElement);
            }

            //no belt? 100% damage regardless, else 50% damage.
            if (DefenseElement ==  Element.None && element != Element.None)
            {
                return 1.00;
            }

            //50% damage.
            else if (DefenseElement == Element.None && element == Element.None)
            {
                return 0.50;
            }

            //fire belt
            if (DefenseElement == Element.Fire)
            {
                if (element == Element.Fire) 
                    return 0.05;
                if (element == Element.Water)
                    return 0.85;
                if (element == Element.Wind)
                    return 0.55;
                if (element == Element.Earth)
                    return 0.65;
                if (element ==  Element.Dark)
                    return 0.75;
                if (element == Element.Light)
                    return 0.55;
                if (element == Element.None)
                    return 0.01;
            }

            //wind belt
            if (DefenseElement == Element.Wind)
            {
                if (element == Element.Wind)
                    return 0.05;
                if (element == Element.Fire)
                    return 0.85;
                if (element == Element.Water)
                    return 0.65;
                if (element == Element.Earth)
                    return 0.55;
                if (element == Element.Dark)
                    return 0.75;
                if (element == Element.Light)
                    return 0.55;
                if (element == Element.None)
                    return 0.01;
            }

            //earth belt
            if (DefenseElement == Element.Earth)
            {
                if (element == Element.Wind)
                    return 0.85;
                if (element == Element.Fire)
                    return 0.65;
                if (element == Element.Water)
                    return 0.55;
                if (element == Element.Earth)
                    return 0.05;
                if (element == Element.Dark)
                    return 0.75;
                if (element == Element.Light)
                    return 0.55;
                if (element == Element.None)
                    return 0.01;
            }


            //water belt
            if (DefenseElement == Element.Water)
            {
                if (element == Element.Wind)
                    return 0.65;
                if (element == Element.Fire)
                    return 0.55;
                if (element == Element.Water)
                    return 0.05;
                if (element == Element.Earth)
                    return 0.85;
                if (element == Element.Dark)
                    return 0.75;
                if (element == Element.Light)
                    return 0.55;
                if (element == Element.None)
                    return 0.01;
            }

            //dark belt
            if (DefenseElement == Element.Dark)
            {
                if (element == Element.Dark)
                    return 0.10;
                if (element == Element.Light)
                    return 0.80;
                if (element == Element.None)
                    return 0.01;

                return 0.60;
            }

            //light Belt
            if (DefenseElement == Element.Light)
            {
                if (element == Element.Dark)
                    return 0.80;
                if (element == Element.Light)
                    return 0.10;
                if (element == Element.None)
                    return 0.01;

                return 0.65;
            }

            return 0.00;
        }

        private int CompleteDamageApplication(int dmg, byte sound, Action<int> dmgcb, double amplifier)
        {
            if (dmg <= 0)
                dmg = 1;

            if (CurrentHp > MaximumHp)
                CurrentHp = MaximumHp;

            var dmg_applied = (int) Math.Abs(dmg * amplifier);

            CurrentHp -= dmg_applied;

            if (CurrentHp < 0)
                CurrentHp = 0;

            var hpbar = new ServerFormat13
            {
                Serial = Serial,
                Health = (ushort) ((double) 100 * CurrentHp / MaximumHp),
                Sound = sound
            };

            Show(Scope.VeryNearbyAislings, hpbar);
            {
                dmgcb?.Invoke(dmg_applied);
            }

            return dmg_applied;
        }

        /// <summary>
        ///     Checks the source of damage and if it's a player, check if the target is a player.
        ///     is true, checks weather or not damage can be applied on the map they are on both on.
        /// </summary>
        /// <param name="Source">Player applying damage.</param>
        /// <returns>true : false</returns>
        public bool CanBeAttackedHere(Sprite Source)
        {
            if (Source is Aisling && this is Aisling)
                if (CurrentMapId > 0 && ServerContext.GlobalMapCache.ContainsKey(CurrentMapId))
                    if (!ServerContext.GlobalMapCache[CurrentMapId].Flags.HasFlag(MapFlags.PlayerKill))
                        return false;

            return true;
        }

        /// <summary>
        ///     Sends Format With Target Scope.
        /// </summary>
        public void Show<T>(Scope op, T format, IEnumerable<Sprite> definer = null) where T : NetworkFormat
        {
            if (Map == null)
                return;


            try
            {
                switch (op)
                {
                    case Scope.Self:
                        Client?.Send(format);
                        break;
                    case Scope.NearbyAislingsExludingSelf:
                        foreach (var gc in GetObjects<Aisling>(Map, that => WithinRangeOf(that)))
                            if (gc.Serial != Serial)
                            {
                                if (this is Aisling)
                                {
                                    if (!gc.Client.Aisling.CanSeeHidden() && (this as Aisling).Invisible)
                                        if (format is ServerFormat33)
                                            return;

                                    if (!gc.Client.Aisling.CanSeeGhosts() && (this as Aisling).Dead)
                                        if (format is ServerFormat33)
                                            return;
                                }

                                gc.Client.Send(format);
                            }

                        break;
                    case Scope.NearbyAislings:
                        foreach (var gc in GetObjects<Aisling>(Map, that => WithinRangeOf(that)))
                        {
                            if (this is Aisling)
                            {
                                if (!gc.Client.Aisling.CanSeeHidden() && (this as Aisling).Invisible)
                                    if (format is ServerFormat33)
                                        return;

                                if (!gc.Client.Aisling.CanSeeGhosts() && (this as Aisling).Dead)
                                    if (format is ServerFormat33)
                                        return;
                            }

                            gc.Client.Send(format);
                        }

                        break;
                    case Scope.VeryNearbyAislings:
                        foreach (var gc in GetObjects<Aisling>(Map, that =>
                            WithinRangeOf(that, ServerContext.Config.VeryNearByProximity)))
                        {
                            if (this is Aisling)
                            {
                                if (!gc.Client.Aisling.CanSeeHidden() && (this as Aisling).Invisible)
                                    if (format is ServerFormat33)
                                        return;

                                if (!gc.Client.Aisling.CanSeeGhosts() && (this as Aisling).Dead)
                                    if (format is ServerFormat33)
                                        return;
                            }

                            gc.Client.Send(format);
                        }

                        break;
                    case Scope.AislingsOnSameMap:
                        foreach (var gc in GetObjects<Aisling>(Map, that => CurrentMapId == that.CurrentMapId))
                        {
                            if (this is Aisling)
                            {
                                if (!gc.Client.Aisling.CanSeeHidden() && (this as Aisling).Invisible)
                                    if (format is ServerFormat33)
                                        return;

                                if (!gc.Client.Aisling.CanSeeGhosts() && (this as Aisling).Dead)
                                    if (format is ServerFormat33)
                                        return;
                            }

                            gc.Client.Send(format);
                        }

                        break;
                    case Scope.GroupMembers:
                    {
                        if (this is Aisling)
                            foreach (var gc in GetObjects<Aisling>(Map, that => (this as Aisling).GroupParty.Has(that)))
                            {
                                if (!gc.Client.Aisling.CanSeeHidden() && (this as Aisling).Invisible)
                                    if (format is ServerFormat33)
                                        return;

                                if (!gc.Client.Aisling.CanSeeGhosts() && (this as Aisling).Dead)
                                    if (format is ServerFormat33)
                                        return;

                                gc.Client.Send(format);
                            }
                    }
                        break;
                    case Scope.NearbyGroupMembersExcludingSelf:
                    {
                        if (this is Aisling)
                            foreach (var gc in GetObjects<Aisling>(Map, that =>
                                that.WithinRangeOf(this) && (this as Aisling).GroupParty.Has(that)))
                            {
                                if (!gc.Client.Aisling.CanSeeHidden() && (this as Aisling).Invisible)
                                    if (format is ServerFormat33)
                                        return;

                                if (!gc.Client.Aisling.CanSeeGhosts() && (this as Aisling).Dead)
                                    if (format is ServerFormat33)
                                        return;

                                gc.Client.Send(format);
                            }
                    }
                        break;
                    case Scope.NearbyGroupMembers:
                    {
                        if (this is Aisling)
                            foreach (var gc in GetObjects<Aisling>(Map, that =>
                                that.WithinRangeOf(this) && (this as Aisling).GroupParty.Has(that, true)))
                            {
                                if (!gc.Client.Aisling.CanSeeHidden() && (this as Aisling).Invisible)
                                    if (format is ServerFormat33)
                                        return;

                                if (!gc.Client.Aisling.CanSeeGhosts() && (this as Aisling).Dead)
                                    if (format is ServerFormat33)
                                        return;

                                gc.Client.Send(format);
                            }
                    }
                        break;
                    case Scope.DefinedAislings:
                        if (definer != null)
                            foreach (var gc in definer)
                            {
                                if (this is Aisling)
                                {
                                    if (!gc.Client.Aisling.CanSeeHidden() && (this as Aisling).Invisible)
                                        if (format is ServerFormat33)
                                            return;

                                    if (!gc.Client.Aisling.CanSeeGhosts() && (this as Aisling).Dead)
                                        if (format is ServerFormat33)
                                            return;
                                }

                                (gc as Aisling).Client.Send(format);
                            }

                        break;
                }
            }
            catch (Exception)
            {
                ServerContext.Logger.Error("Error in Show<T>");
            }
        }

        public Aisling Aisling(Sprite obj)
        {
            if (obj is Aisling aisling)
                return aisling;

            return null;
        }

        public Monster Monster(Sprite obj)
        {
            if (obj is Monster monster)
                return monster;

            return null;
        }


        /// <summary>
        /// See Formula Applied : DarkAges-Lorule-Server\Tools\ACDamageFormula.xlsx"
        /// </summary>
        private int ComputeDmgFromAc(int dmg)
        {
            var armor          = Ac;
            var calculated_dmg = dmg * Math.Abs(armor + 101) / 99;

            if (calculated_dmg < 0)
                calculated_dmg = 1;

            var diff = Math.Abs(dmg - calculated_dmg);

            return calculated_dmg + diff;
        }


        public IEnumerable<Sprite> GetSprites(int x, int y)
        {
            return GetObjects(Map, i => i.XPos == x && i.YPos == y, Get.All);
        }


        public List<Sprite> GetInfront(Sprite sprite, int tileCount = 1)
        {
            return _GetInfront(tileCount).Where(i => i != null && i.Serial != sprite.Serial).ToList();
        }

        public List<Sprite> GetInfront(int tileCount = 1, bool intersect = false)
        {
            return _GetInfront(tileCount).ToList();
        }

        private List<Sprite> _GetInfront(int tileCount = 1)
        {
            var results = new List<Sprite>();

            for (var i = 1; i <= tileCount; i++)
                switch (Direction)
                {
                    case 0:
                        results.AddRange(GetSprites(XPos, YPos - i));
                        break;
                    case 1:
                        results.AddRange(GetSprites(XPos + i, YPos));
                        break;
                    case 2:
                        results.AddRange(GetSprites(XPos, YPos + i));
                        break;
                    case 3:
                        results.AddRange(GetSprites(XPos - i, YPos));
                        break;
                }

            return results;
        }

        public void HideFrom(Aisling nearbyAisling)
        {
            nearbyAisling.Show(Scope.Self, new ServerFormat0E(Serial));
        }

        public void ShowTo(Aisling nearbyAisling)
        {
            if (nearbyAisling != null)
            {
                if (this is Aisling)
                    nearbyAisling.Show(Scope.Self, new ServerFormat33(Client, this as Aisling));
                else
                    nearbyAisling.Show(Scope.Self, new ServerFormat07(new[] {this}));
            }
        }

        public bool WithinRangeOf(Sprite other, bool checkMap = true)
        {
            if (other == null)
                return false;

            return WithinRangeOf(other, ServerContext.Config.WithinRangeProximity, checkMap);
        }

        public bool WithinRangeOf(Sprite other, int distance, bool checkMap = true)
        {
            if (other == null)
                return false;

            if (checkMap)
                if (CurrentMapId != other.CurrentMapId)
                    return false;


            return WithinRangeOf(other.XPos, other.YPos, distance);
        }

        public bool WithinRangeOf(int x, int y, int subjectLength)
        {
            var A = new Point(XPos, YPos);
            var B = new Point(x, y);
            var Dst = Point.Subtract(A, B).Length;

            return (int) Dst <= subjectLength;
        }

        public bool Facing(Sprite other, out int direction)
        {
            return Facing(other.XPos, other.YPos, out direction);
        }

        public bool Facing(int x, int y, out int direction)
        {
            var xDist = (x - XPos).Clamp(-1, +1);
            var yDist = (y - YPos).Clamp(-1, +1);

            direction = DirectionTable[xDist + 1][yDist + 1];
            return Direction == direction;
        }


        public void Remove()
        {
            if (this is Monster)
                Remove<Monster>();

            if (this is Aisling)
                Remove<Aisling>();

            if (this is Money)
                Remove<Money>();

            if (this is Item)
                Remove<Item>();

            if (this is Mundane)
                Remove<Mundane>();
        }

        public Aisling[] AislingsNearby()
        {
            return GetObjects<Aisling>(Map, i => i != null && i.WithinRangeOf(this)).ToArray();
        }

        public Monster[] MonstersNearby()
        {
            return GetObjects<Monster>(Map, i => i != null && i.WithinRangeOf(this)).ToArray();
        }

        public Mundane[] MundanesNearby()
        {
            return GetObjects<Mundane>(Map, i => i != null && i.WithinRangeOf(this)).ToArray();
        }

        /// <summary>
        ///     Use this to Remove Sprites
        ///     It will remove them from ingame to who those effected.
        ///     and invoke the objectmanager.
        /// </summary>
        public void Remove<T>() where T : Sprite, new()
        {
            var nearby = GetObjects<Aisling>(Map, i => i.WithinRangeOf(this));
            var response = new ServerFormat0E(Serial);

            foreach (var o in nearby) o?.Client?.Send(response);

            DeleteObject();
        }

        private void DeleteObject()
        {
            if (this is Monster)
                DelObject(this as Monster);
            if (this is Aisling)
                DelObject(this as Aisling);
            if (this is Money)
                DelObject(this as Money);
            if (this is Item)
                DelObject(this as Item);
            if (this is Mundane)
                DelObject(this as Mundane);
        }

        public void UpdateBuffs(TimeSpan elapsedTime)
        {
            Buff[] buff_Copy;

            lock (syncLock)
            {
                buff_Copy = new List<Buff>(Buffs.Values).ToArray();
            }

            if (buff_Copy.Length == 0)
                return;

            for (var i = 0; i < buff_Copy.Length; i++)
                if (buff_Copy[i] != null)
                    buff_Copy[i].Update(this, elapsedTime);
        }

        public void UpdateDebuffs(TimeSpan elapsedTime)
        {
            Debuff[] debuff_Copy;

            if (Debuffs == null)
                return;

            if (Debuffs.Count == 0)
                return;

            lock (syncLock)
            {
                debuff_Copy = new List<Debuff>(Debuffs.Values).ToArray();
            }

            if (debuff_Copy.Length == 0)
                return;

            for (var i = 0; i < debuff_Copy.Length; i++)
                if (debuff_Copy[i] != null)
                    debuff_Copy[i].Update(this, elapsedTime);
        }

        /// <summary>
        ///     Show all nearby aislings, this sprite has turned.
        /// </summary>
        public virtual void Turn()
        {
            if (!CanUpdate())
                return;

            if (LastDirection != Direction)
            {
                LastDirection = Direction;

                Show(Scope.NearbyAislings, new ServerFormat11
                {
                    Direction = Direction,
                    Serial = Serial
                });
            }
        }

        public void WalkTo(int x, int y, bool ignoreWalls = false)
        {
            if (!CanUpdate()) return;

            try
            {
                var buffer = new byte[2];
                var length = float.PositiveInfinity;
                var offset = 0;

                for (byte i = 0; i < 4; i++)
                {
                    var newX = XPos + Directions[i][0];
                    var newY = YPos + Directions[i][1];

                    if (newX == x &&
                        newY == y)
                        continue;

                    if (!ignoreWalls && Map.IsWall(this, newX, newY))
                        continue;

                    var xDist = x - newX;
                    var yDist = y - newY;
                    var tDist = (float) Math.Sqrt(xDist * xDist + yDist * yDist);

                    if (length < tDist)
                        continue;

                    if (length > tDist)
                    {
                        length = tDist;
                        offset = 0;
                    }

                    if (offset < buffer.Length)
                        buffer[offset] = i;

                    offset++;
                }

                if (offset == 0)
                    return;

                lock (rnd)
                {
                    if (offset < buffer.Length)
                        Direction = buffer[rnd.Next(0, offset)];
                }

                if (!Walk())
                    return;
            }
            catch
            {
                // ignored
            }
        }

        public virtual void Wander()
        {
            if (!CanUpdate())
                return;

            var savedDirection = Direction;
            var update = false;

            lock (rnd)
            {
                Direction = (byte) rnd.Next(0, 4);

                if (Direction != savedDirection) update = true;
            }

            if (!Walk() && update)
                Show(Scope.NearbyAislings, new ServerFormat11
                {
                    Direction = Direction,
                    Serial = Serial
                });
        }

        public bool CanUpdate()
        {
            if (IsSleeping || IsFrozen || IsBlind)
                return false;

            if (this is Monster || this is Mundane)
                if (CurrentHp == 0)
                    return false;

            if (!ServerContext.Config.CanMoveDuringReap)
                if (this is Aisling _aisling)
                {
                    if (_aisling.Skulled)
                    {
                        _aisling.Client.SystemMessage(ServerContext.Config.ReapMessageDuringAction);
                        return false;
                    }
                }

            return true;
        }


        public virtual bool Walk()
        {
            var savedX   = XPos;
            var savedY   = YPos;
            var pendingX = XPos;
            var pendingY = YPos;
            var result   = TryWalk(pendingX, pendingY, savedX, savedY);

            TriggerNearbyTraps();
            return result;
        }

        public bool TryWalk(int pendingX, int pendingY, int savedX, int savedY)
        {
            if (Direction == 0)
            {
                var canWalk = this is Aisling ? !Map.IsWall(this as Aisling, XPos, YPos - 1) : !Map.IsWall(this, XPos, YPos - 1);

                if (!canWalk)
                    return false;

                pendingY--;
            }

            if (Direction == 1)
            {
                var canWalk = this is Aisling ? !Map.IsWall(this as Aisling, XPos + 1, YPos) : !Map.IsWall(this, XPos + 1, YPos);

                if (!canWalk)
                    return false;

                pendingX++;
            }

            if (Direction == 2)
            {
                var canWalk = this is Aisling ? !Map.IsWall(this as Aisling, XPos, YPos + 1) : !Map.IsWall(this, XPos, YPos + 1);

                if (!canWalk)
                    return false;

                pendingY++;
            }

            if (Direction == 3)
            {
                var canWalk = this is Aisling ? !Map.IsWall(this as Aisling, XPos - 1, YPos) : !Map.IsWall(this, XPos - 1, YPos);

                if (!canWalk)
                    return false;

                pendingX--;
            }

            pendingX = pendingX.Clamp(0, Map.Cols - 1);
            pendingY = pendingY.Clamp(0, Map.Rows - 1);


            CompleteWalk(pendingX, pendingY, savedX, savedY);


            var response = new ServerFormat0C
            {
                Direction = Direction,
                Serial = Serial,
                X = (short)savedX,
                Y = (short)savedY
            };

            XPos = pendingX;
            YPos = pendingY;

            Show(Scope.NearbyAislingsExludingSelf, response);

            if (LastPosition.X != XPos)
                LastPosition.X = (ushort)XPos;

            if (LastPosition.Y != YPos)
                LastPosition.Y = (ushort)YPos;

            return true;
        }

        private bool CompleteWalk(int pendingX, int pendingY, int savedX, int savedY)
        {
            if (this is Aisling)
            {
                Client.Send(new ServerFormat0B
                {
                    Direction = Direction,
                    LastX     = (ushort) savedX,
                    LastY     = (ushort) savedY
                });

            }

            return true;
        }

        public Aisling SendAnimation(ushort Animation, Sprite To, Sprite From, byte speed = 100)
        {
            var format = new ServerFormat29((uint) From.Serial, (uint) To.Serial, Animation, 0, speed);
            {
                Show(Scope.NearbyAislings, format);
            }

            return Aisling(this);
        }

        public void Animate(ushort animation, byte speed = 100)
        {
            Show(Scope.NearbyAislings, new ServerFormat29((uint) Serial, (uint) Serial, animation, animation, speed));
        }

        public void BarMsg(string message, byte type = 0x02)
        {
            var response = new ServerFormat0D
            {
                Serial = Serial,
                Type = type,
                Text = message
            };

            Show(Scope.NearbyAislings, response);
        }

        public void GiveHP(int value)
        {
            _MaximumHp += value;
        }

        public void GiveMP(int value)
        {
            _MaximumMp += value;
        }

        public void Kill()
        {
            CurrentHp = 0;
        }

        public void Update()
        {
            Show(Scope.NearbyAislings, new ServerFormat0E(Serial));
            Show(Scope.NearbyAislings, new ServerFormat07(new[] {this}));
        }

        public void ScrollTo(string destination, short x, short y)
        {
            var map = ServerContext.GlobalMapCache.Where(i =>
                i.Value.Name.Equals(destination, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (map.Value != null)
                if (this is Aisling)
                {
                    var client = (this as Aisling).Client;

                    client.TransitionToMap(map.Value, new Position(x, y));
                }
        }

        public void SendAnimation(ushort v, Position position)
        {
            Show(Scope.NearbyAislings, new ServerFormat29(v, position.X, position.Y));
        }

        public Sprite ApplyBuff(string buff)
        {
            if (ServerContext.GlobalBuffCache.ContainsKey(buff))
            {
                var Buff = Clone<Buff>(ServerContext.GlobalBuffCache[buff]);

                if (Buff == null || string.IsNullOrEmpty(Buff.Name))
                    return null;

                if (!HasBuff(Buff.Name))
                    Buff.OnApplied(this, Buff);
            }

            return this;
        }

        public Sprite ApplyDebuff(string debuff)
        {
            if (ServerContext.GlobalDeBuffCache.ContainsKey(debuff))
            {
                var Debuff = Clone<Debuff>(ServerContext.GlobalDeBuffCache[debuff]);
                if (!HasDebuff(Debuff.Name))
                    Debuff.OnApplied(this, Debuff);
            }

            return this;
        }

        public void RefreshStats()
        {
            if (this is Aisling)
                (this as Aisling).Client.SendStats(StatusFlags.All);
        }

        public void WarpTo(Position newLocation)
        {
            var location = new Position(newLocation.X, newLocation.Y);

            X = location.X;
            Y = location.Y;

            Map.Update(X, Y);
            Update();
        }
        #endregion
    }
}