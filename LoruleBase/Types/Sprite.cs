using Darkages.Common;
using Darkages.Network;
using Darkages.Network.Game;
using Darkages.Network.Object;
using Darkages.Network.ServerFormats;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Darkages.Network.Game.Components;
using static Darkages.Types.ElementManager;

namespace Darkages.Types
{
    public abstract class Sprite : ObjectManager, INotifyPropertyChanged
    {
        [JsonIgnore] public byte LastDirection;

        [JsonIgnore] public Position LastPosition;

        protected Sprite()
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
            LastTurnUpdated = DateTime.UtcNow;
            LastUpdated = DateTime.UtcNow;

            LastPosition = new Position(0, 0);
            LastDirection = 0;
        }

        [JsonIgnore] public GameClient Client { get; set; }

        [JsonIgnore] public Area Map => ServerContextBase.GlobalMapCache.ContainsKey(CurrentMapId)
            ? ServerContextBase.GlobalMapCache[CurrentMapId]
            : null;

        [JsonIgnore] public TileContent EntityType { get; set; }
                     
        [JsonIgnore] public Sprite Target { get; set; }
                     
        [JsonIgnore] public Position Position => new Position(XPos, YPos);
                     
        [JsonIgnore] public bool Attackable => this is Monster || this is Aisling || this is Mundane;
                     
        [JsonIgnore] public bool Alive => CurrentHp > 0;
                     
        [JsonIgnore] public DateTime AbandonedDate { get; set; }
                     
        [JsonIgnore] public DateTime LastUpdated { get; set; }
                     
        [JsonIgnore] public DateTime LastTargetAcquired { get; set; }
                     
        [JsonIgnore] public DateTime LastMovementChanged { get; set; }
                     
        [JsonIgnore] public DateTime LastMenuInvoked { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public int Level => EntityType == TileContent.Aisling ? ((Aisling) this).ExpLevel
            : EntityType == TileContent.Monster ? ((Monster) this).Template.Level
            : EntityType == TileContent.Mundane ? ((Mundane) this).Template.Level
            : EntityType == TileContent.Item ? ((Item) this).Template.LevelRequired : 0;


        public ConcurrentDictionary<string, Debuff> Debuffs { get; set; }

        public ConcurrentDictionary<string, Buff> Buffs { get; set; }

        public Element OffenseElement { get; set; }

        public Element DefenseElement { get; set; }

        public PrimaryStat MajorAttribute { get; set; }

        public byte Direction { get; set; }

        public int CurrentMapId { get; set; }

        public int Amplified { get; set; }

        public DateTime LastTurnUpdated { get; set; }

        [JsonIgnore] public bool CanMove => !(IsFrozen || IsSleeping || IsParalyzed);
                     
        [JsonIgnore] public bool CanCast => !(IsFrozen || IsSleeping);
                     
        [JsonIgnore] public bool EmpoweredAssail { get; set; }

        public bool Immunity { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Identification & Position
        
        public int Serial { get; set; }

        public int X;

        public int Y;

        [JsonIgnore]
        public int XPos
        {
            get => X;
            set
            {
                if (X == value)
                    return;

                X = value;
                NotifyPropertyChanged();
            }
        }

        [JsonIgnore]
        public int YPos
        {
            get => Y;
            set
            {
                if (Y == value)
                    return;

                Y = value;

                NotifyPropertyChanged();
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

        [JsonIgnore] public int Regen => (_Regen + BonusRegen).Clamp(0, 300);

        [JsonIgnore] public int MaximumHp => _MaximumHp + BonusHp;

        [JsonIgnore] public int MaximumMp => _MaximumMp + BonusMp;

        [JsonIgnore]
        public byte Str
        {
            get
            {
                var tmp = (byte) (_Str + BonusStr).Clamp(1, byte.MaxValue);
                return tmp > 255 ? (byte) 255 : tmp;
            }
        }

        [JsonIgnore]
        public byte Int
        {
            get
            {
                var tmp = (byte) (_Int + BonusInt).Clamp(1, byte.MaxValue);
                return tmp > 255 ? (byte) 255 : tmp;
            }
        }

        [JsonIgnore]
        public byte Wis
        {
            get
            {
                var tmp = (byte) (_Wis + BonusWis).Clamp(1, byte.MaxValue);
                return tmp > 255 ? (byte) 255 : tmp;
            }
        }

        [JsonIgnore]
        public byte Con
        {
            get
            {
                var tmp = (byte) (_Con + BonusCon).Clamp(1, byte.MaxValue);
                return tmp > 255 ? (byte) 255 : tmp;
            }
        }

        [JsonIgnore]
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

        [JsonIgnore]
        public int Ac
        {
            get
            {
                if (BonusAc < -70)
                    return -70;

                return BonusAc;
            }
        }

        [JsonIgnore] public byte Mr => (byte) (_Mr + BonusMr).Clamp(0, 70);

        [JsonIgnore] public byte Dmg => (byte) (_Dmg + BonusDmg).Clamp(0, byte.MaxValue);

        [JsonIgnore] public byte Hit => (byte) (_Hit + BonusHit).Clamp(0, byte.MaxValue);

        [JsonIgnore] public int BonusStr { get; set; }

        [JsonIgnore] public int BonusInt { get; set; }

        [JsonIgnore] public int BonusWis { get; set; }

        [JsonIgnore] public int BonusCon { get; set; }

        [JsonIgnore] public int BonusDex { get; set; }

        [JsonIgnore] public byte BonusMr { get; set; }

        [JsonIgnore] public int BonusAc { get; set; }

        [JsonIgnore] public byte BonusHit { get; set; }

        [JsonIgnore] public byte BonusDmg { get; set; }

        [JsonIgnore] public int BonusHp { get; set; }

        [JsonIgnore] public int BonusMp { get; set; }

        [JsonIgnore] public int BonusRegen { get; set; }

        #endregion

        #region Status

        [JsonIgnore] public bool IsAited => HasBuff("aite");
                     
        [JsonIgnore] public bool IsSleeping => HasDebuff("sleep");
                     
        [JsonIgnore] public bool IsFrozen => HasDebuff("frozen");
                     
        [JsonIgnore] public bool IsPoisoned => HasDebuff(i => i.Name.ToLower().Contains("puinsein"));
                     
        [JsonIgnore] public bool IsCursed => HasDebuff(i => i.Name.ToLower().Contains("cradh"));
                     
        [JsonIgnore] public bool IsBleeding => HasDebuff("bleeding");
                     
        [JsonIgnore] public bool IsBlind => HasDebuff("blind");
                     
        [JsonIgnore] public bool IsConfused => HasDebuff("confused");

        [JsonIgnore] public bool IsParalyzed => HasDebuff("paralyze") || HasDebuff(i => i.Name.ToLower().Contains("beag suain"));


        [JsonIgnore] public static int[][] Directions { get; } =
        {
            new[] {+0, -1},
            new[] {+1, +0},
            new[] {+0, +1},
            new[] {-1, +0}
        };

        [JsonIgnore] public static int[][] DirectionTable { get; } =
        {
            new[] {-1, +3, -1},
            new[] {+0, -1, +2},
            new[] {-1, +1, -1}
        };

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
                var mod = 0.0;
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
            if (Buffs == null) 
                return;

            foreach (var buff in Buffs)
                RemoveBuff(buff.Key);
        }

        public void RemoveAllDebuffs()
        {
            if (Debuffs == null) 
                return;

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

        public bool CanTag(Aisling attackingPlayer, bool force = false)
        {
            var canTag = false;

            if (!(this is Monster monster))
                return false;

            if (monster.TaggedAislings.Any(i => i == attackingPlayer.Serial))
                canTag = true;

            if (monster.TaggedAislings.Count == 0)
                canTag = true;

            var tagstoRemove = new List<int>();
            foreach (var userId in monster.TaggedAislings.Where(i => i != attackingPlayer.Serial))
            {
                var taggeduser = GetObject<Aisling>(Map, i => i.Serial == userId);

                if (taggeduser != null)
                {
                    if (taggeduser.WithinRangeOf(this))
                    {
                        canTag = attackingPlayer.GroupParty.Has(taggeduser);
                    }
                    else
                    {
                        tagstoRemove.Add(taggeduser.Serial);
                        canTag = true;
                    }
                }
            }

            var lostTags = monster.AislingsNearby().Where(i => monster.TaggedAislings.Contains(i.Serial));

            if (!lostTags.Any()) canTag = true;


            monster.TaggedAislings.RemoveWhere(n => tagstoRemove.Contains(n));


            if (canTag)
            {
                monster.AppendTags(attackingPlayer);

                if (monster.Target == null)
                    monster.Target = attackingPlayer;
            }

            if (force) canTag = false;

            return canTag;
        }

        public void ApplyDamage(Sprite damageDealingSprite, int dmg, bool penetrating = false, byte sound = 1,
            Action<int> dmgcb = null, bool forceTarget = false)
        {
            if (!WithinRangeOf(damageDealingSprite))
                return;

            if (!Attackable)
                return;

            if (!CanBeAttackedHere(damageDealingSprite))
                return;

            if (Map.Flags.HasFlag(MapFlags.PlayerKill))
            {
                //normalize pvp damage.
                dmg = (int)(dmg * 0.75);
            }

            if (dmg == -1)
            {
                dmg = CurrentHp;
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

            if (!(source is Aisling aisling))
                return;

            var monsterScripts = (this as Monster)?.Scripts;

            if (monsterScripts == null)
                return;

            foreach (var script in monsterScripts?.Values)
                script?.OnDamaged(aisling?.Client, dmg, source);
        }

        private bool DamageTarget(Sprite damageDealingSprite,
            ref int dmg, bool penetrating, byte sound,
            Action<int> dmgcb, bool forced)
        {

            #region Direct Damage
            if (penetrating)
            {
                var empty = new ServerFormat13
                {
                    Serial = Serial,
                    Health = byte.MaxValue,
                    Sound = sound
                };

                Show(Scope.VeryNearbyAislings, empty);

                CurrentHp -= dmg;

                if (CurrentHp < 0)
                    CurrentHp = 0;

                return true;
            }
            #endregion


            if (this is Monster)
                if (damageDealingSprite is Aisling aisling)
                    if (!CanTag(aisling, forced))
                    {
                        aisling.Client.SendMessage(0x02, ServerContextBase.GlobalConfig.CantAttack);
                        return false;
                    }

            if (Immunity)
            {
                var empty = new ServerFormat13
                {
                    Serial = Serial,
                    Health = byte.MaxValue,
                    Sound = sound
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

            return true;
        }

        private int ApplyWeaponBonuses(Sprite source, int dmg)
        {
            if (!(source is Aisling aisling))
                return dmg;

            if (aisling.EquipmentManager.Weapon?.Item == null || aisling.Weapon <= 0)
                return dmg;

            var weapon = aisling.EquipmentManager.Weapon.Item;

            lock (Generator.Random)
            {
                dmg += Generator.Random.Next(
                    weapon.Template.DmgMin + (aisling.BonusDmg * 1),
                    weapon.Template.DmgMax + (aisling.BonusDmg * 5));
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

            var amplifier = CalculateElementalDamageMod(element);
            {
                DefenseElement = saved;
            }

            amplifier *= Amplified == 1
                ? ServerContextBase.GlobalConfig.FasNadurStrength + 10
                : ServerContextBase.GlobalConfig.MorFasNadurStrength + 30;


            return amplifier;
        }

        private double CalculateElementalDamageMod(Element element)
        {
            while (DefenseElement == Element.Random) DefenseElement = CheckRandomElement(DefenseElement);

            //no belt? 100% damage regardless, else 50% damage.
            if (DefenseElement == Element.None && element != Element.None)
                return 1.00;

            //50% damage.
            if (DefenseElement == Element.None && element == Element.None) return 0.50;

            //fire belt
            if (DefenseElement == Element.Fire)
            {
                switch (element)
                {
                    case Element.Fire:
                        return 0.05;
                    case Element.Water:
                        return 0.85;
                    case Element.Wind:
                        return 0.55;
                    case Element.Earth:
                        return 0.65;
                    case Element.Dark:
                        return 0.75;
                    case Element.Light:
                        return 0.55;
                    case Element.None:
                        return 0.01;
                }
            }

            //wind belt
            if (DefenseElement == Element.Wind)
            {
                switch (element)
                {
                    case Element.Wind:
                        return 0.05;
                    case Element.Fire:
                        return 0.85;
                    case Element.Water:
                        return 0.65;
                    case Element.Earth:
                        return 0.55;
                    case Element.Dark:
                        return 0.75;
                    case Element.Light:
                        return 0.55;
                    case Element.None:
                        return 0.01;
                }
            }

            //earth belt
            if (DefenseElement == Element.Earth)
            {
                switch (element)
                {
                    case Element.Wind:
                        return 0.85;
                    case Element.Fire:
                        return 0.65;
                    case Element.Water:
                        return 0.55;
                    case Element.Earth:
                        return 0.05;
                    case Element.Dark:
                        return 0.75;
                    case Element.Light:
                        return 0.55;
                    case Element.None:
                        return 0.01;
                }
            }


            //water belt
            if (DefenseElement == Element.Water)
            {
                switch (element)
                {
                    case Element.Wind:
                        return 0.65;
                    case Element.Fire:
                        return 0.55;
                    case Element.Water:
                        return 0.05;
                    case Element.Earth:
                        return 0.85;
                    case Element.Dark:
                        return 0.75;
                    case Element.Light:
                        return 0.55;
                    case Element.None:
                        return 0.01;
                }
            }

            //dark belt
            if (DefenseElement == Element.Dark)
            {
                switch (element)
                {
                    case Element.Dark:
                        return 0.10;
                    case Element.Light:
                        return 0.80;
                    case Element.None:
                        return 0.01;
                    default:
                        return 0.60;
                }
            }

            //light Belt
            if (DefenseElement == Element.Light)
            {
                switch (element)
                {
                    case Element.Dark:
                        return 0.80;
                    case Element.Light:
                        return 0.10;
                    case Element.None:
                        return 0.01;
                    default:
                        return 0.65;
                }
            }

            return 0.00;
        }

        private int CompleteDamageApplication(int dmg, byte sound, Action<int> dmgcb, double amplifier)
        {
            if (dmg <= 0)
                dmg = 1;

            if (CurrentHp > MaximumHp)
                CurrentHp = MaximumHp;

            var dmgApplied = (int) Math.Abs(dmg * amplifier);

            CurrentHp -= dmgApplied;

            if (CurrentHp < 0)
                CurrentHp = 0;

            var hpbar = new ServerFormat13
            {
                Serial = Serial,
                Health = (ushort) ((double) 100 * CurrentHp / MaximumHp),
                Sound  = sound
            };

            Show(Scope.VeryNearbyAislings, hpbar);
            {
                dmgcb?.Invoke(dmgApplied);
            }

            return dmgApplied;
        }


        public bool CanBeAttackedHere(Sprite source)
        {
            if (!(source is Aisling) || !(this is Aisling))
                return true;

            if (CurrentMapId <= 0 || !ServerContextBase.GlobalMapCache.ContainsKey(CurrentMapId))
                return true;

            return ServerContextBase.GlobalMapCache[CurrentMapId].Flags.HasFlag(MapFlags.PlayerKill);
        }


        public void Show<T>(Scope op, T format, IEnumerable<Sprite> definer = null) where T : NetworkFormat
        {
            if (Map == null)
                return;

            try
            {
                if (op == Scope.Self)
                {
                    Client?.Send(format);
                }
                else if (op == Scope.NearbyAislingsExludingSelf)
                {
                    foreach (var gc in GetObjects<Aisling>(Map, that => WithinRangeOf(that)))
                        if (gc.Serial != Serial)
                        {
                            gc.Client.Send(format);
                        }
                }
                else if (op == Scope.NearbyAislings)
                {
                    foreach (var gc in GetObjects<Aisling>(Map, that => WithinRangeOf(that)))
                    {
                        gc.Client.Send(format);
                    }
                }
                else if (op == Scope.VeryNearbyAislings)
                {
                    foreach (var gc in GetObjects<Aisling>(Map, that =>
                        WithinRangeOf(that, ServerContextBase.GlobalConfig.VeryNearByProximity)))
                    {
                        gc.Client.Send(format);
                    }
                }
                else if (op == Scope.AislingsOnSameMap)
                {
                    foreach (var gc in GetObjects<Aisling>(Map, that => CurrentMapId == that.CurrentMapId))
                    {
                        gc.Client.Send(format);
                    }
                }
                else if (op == Scope.GroupMembers)
                {
                    if (!(this is Aisling))
                        return;

                    foreach (var gc in GetObjects<Aisling>(Map, that => ((Aisling) this).GroupParty.Has(that)))
                    {
                        gc.Client.Send(format);
                    }
                }
                else if (op == Scope.NearbyGroupMembersExcludingSelf)
                {
                    if (!(this is Aisling))
                        return;

                    foreach (var gc in GetObjects<Aisling>(Map, that => that.WithinRangeOf(this) && ((Aisling) this).GroupParty.Has(that)))
                    {
                        gc.Client.Send(format);
                    }

                }
                else if (op == Scope.NearbyGroupMembers)
                {
                    if (!(this is Aisling))
                        return;

                    foreach (var gc in GetObjects<Aisling>(Map, that => that.WithinRangeOf(this) && ((Aisling) this).GroupParty.Has(that, true)))
                    {
                        gc.Client.Send(format);
                    }
                }
                else if (op == Scope.DefinedAislings)
                {
                    if (definer == null)
                        return;

                    foreach (var gc in definer)
                    {
                        (gc as Aisling)?.Client.Send(format);
                    }
                }
                else if (op == Scope.All)
                {

                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(op), op, null);
                }
            }
            catch (Exception)
            {

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
        /// See Formula Applied : DarkAges-Lorule-Client\Tools\ACDamageFormula.xlsx"
        /// </summary>
        private int ComputeDmgFromAc(int dmg)
        {
            var armor = Ac;
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
                {
                    nearbyAisling.Show(Scope.Self, new ServerFormat33(Client, this as Aisling));
                }
                else
                    nearbyAisling.Show(Scope.Self, new ServerFormat07(new[] {this}));
            }
        }

        public bool WithinRangeOf(Sprite other, bool checkMap = true)
        {
            return other != null && WithinRangeOf(other, ServerContextBase.GlobalConfig.WithinRangeProximity, checkMap);
        }

        public bool WithinRangeOf(Sprite other, int distance, bool checkMap = true)
        {
            if (other == null)
                return false;

            if (!checkMap)
                return WithinRangeOf(other.XPos, other.YPos, distance);

            return CurrentMapId == other.CurrentMapId && WithinRangeOf(other.XPos, other.YPos, distance);
        }

        public bool WithinRangeOf(int x, int y, int subjectLength)
        {
            return DistanceFrom(x, y) < subjectLength;
        }

        public int DistanceFrom(int x, int y)
        {
            return (Math.Abs(this.X - x) + Math.Abs(this.Y - y));
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
            var nearby   = GetObjects<Aisling>(Map, i => i.CurrentMapId == CurrentMapId);
            var response = new ServerFormat0E(Serial);

            foreach (var o in nearby)
            {
                for (var i = 0; i < 3; i++)
                {
                    o?.Client?.FlushAndSend(response);
                }
            }

            DeleteObject();
        }

        public Aisling[] AislingsNearby() => GetObjects<Aisling>(Map, i => i != null && i.WithinRangeOf(this)).ToArray();
        public Monster[] MonstersNearby() => GetObjects<Monster>(Map, i => i != null && i.WithinRangeOf(this)).ToArray();
        public Mundane[] MundanesNearby() => GetObjects<Mundane>(Map, i => i != null && i.WithinRangeOf(this)).ToArray();

        private static readonly int[][] directions = new int[][]
        {
            new int[] { +0, -1 },
            new int[] { +1, +0 },
            new int[] { +0, +1 },
            new int[] { -1, +0 },
        };

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
            var buff_Copy = new List<Buff>(Buffs.Values).ToArray();

            if (buff_Copy.Length == 0)
                return;

            for (var i = 0; i < buff_Copy.Length; i++)
                if (buff_Copy[i] != null)
                    buff_Copy[i].Update(this, elapsedTime);
        }

        public void UpdateDebuffs(TimeSpan elapsedTime)
        {
            if (Debuffs == null)
                return;

            if (Debuffs.Count == 0)
                return;

            var debuff_Copy = new List<Debuff>(Debuffs.Values).ToArray();

            if (debuff_Copy.Length == 0)
                return;

            for (var i = 0; i < debuff_Copy.Length; i++)
                if (debuff_Copy[i] != null)
                    debuff_Copy[i].Update(this, elapsedTime);
        }

        public virtual void Turn()
        {
            if (!CanUpdate())
                return;

            Show(Scope.NearbyAislings, new ServerFormat11
            {
                Direction = Direction,
                Serial = Serial
            });

            LastDirection = Direction;
            LastTurnUpdated = DateTime.UtcNow;
        }

        public bool NextTo(int x, int y)
        {
            int xDist = Math.Abs(x - this.X);
            int yDist = Math.Abs(y - this.Y);

            return (xDist + yDist) == 1;
        }
        public static float Sqrt(float number)
        {
            ulong i;
            float x = (number * 0.5f);
            float y = (number);

            unsafe
            {
                i = *(ulong*)&y;
                i = (0x5F3759DF - (i >> 1));
                y = *(float*)&i;
                y = (y * (1.5f - (x * y * y)));
                y = (y * (1.5f - (x * y * y)));
            }

            return (number * y);
        }

        public bool WalkTo(int x, int y, bool ignoreWalls = false)
        {
            var buffer = new byte[2];
            var length = float.PositiveInfinity;
            var offset = 0;

            for (byte i = 0; i < 4; i++)
            {
                var newX = (this.X + directions[i][0]);
                var newY = (this.Y + directions[i][1]);

                if (newX == x &&
                    newY == y)
                    return false;

               if (this.Map.IsWall(newX, newY)) 
                   continue;

               if (GetObjects(Map, n => n.Serial == Serial && n.X == newX && n.Y == newY, Get.Monsters | Get.Aislings | Get.Mundanes).Any())
                   continue;

                var xDist = (x - newX);
                var yDist = (y - newY);
                var tDist = Sqrt(xDist * xDist + yDist * yDist);

                if (length < tDist)
                    continue;

                if (length > tDist)
                {
                    length = tDist;
                    offset = 0;
                }

                buffer[offset] = i;
                offset++;
            }

            if (offset == 0)
                return false;

            lock (Generator.Random)
            {
                var pendingDirection = buffer[Generator.Random.Next(0, offset)];
                Direction = pendingDirection;

                return this.Walk();
            }
        }

        readonly Random _rnd = new Random();

        public virtual void Wander()
        {
            if (!CanUpdate())
                return;

            var savedDirection = Direction;
            var update = false;

            lock (_rnd)
            {
                Direction = (byte) _rnd.Next(0, 4);

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

            if (ServerContextBase.GlobalConfig.CanMoveDuringReap)
                return true;

            if (!(this is Aisling aisling))
                return true;

            if (!aisling.Skulled)
                return true;

            aisling.Client.SystemMessage(ServerContextBase.GlobalConfig.ReapMessageDuringAction);
            return false;
        }

        public virtual Position GetPendingWalkPosition()
        {
            var pendingX = X;
            var pendingY = Y;

            if (Direction == 0)
                pendingY--;

            if (Direction == 1)
                pendingX++;

            if (Direction == 2)
                pendingY++;

            if (Direction == 3)
                pendingX--;

            return new Position(pendingX, pendingY);
        }

        public virtual bool Walk()
        {
            var savedX   = this.X;
            var savedY   = this.Y;
            var pendingX = this.X;
            var pendingY = this.Y;


            if (Map.IsWall(savedX, savedY, this is Aisling))
                return false;

            if (!Map.ObjectGrid[savedX, savedY].IsPassable(this, this is Aisling))
                return false;



            if (this.Direction == 0)
                pendingY--;
            else if (this.Direction == 1)
                pendingX++;
            else if (this.Direction == 2)
                pendingY++;
            else if (this.Direction == 3)
                pendingX--;


            if (Map.IsWall(pendingX, pendingY, this is Aisling))
                return false;

            if (!Map.ObjectGrid[pendingX, pendingY].IsPassable(this, this is Aisling))
                return false;

            var response = new ServerFormat0C
            {
                Direction = this.Direction,
                Serial = Serial,
                X = (short) savedX,
                Y = (short) savedY
            };

            X = pendingX;
            Y = pendingY;

            Show(Scope.NearbyAislingsExludingSelf, response);
            {
                LastMovementChanged = DateTime.UtcNow;
                LastPosition = new Position(savedX, savedY);
            }

            if (this is Aisling)
            {
                foreach (var obj in AislingsNearby())
                {
                    ObjectComponent.UpdateClientObjects(obj);
                }
            }

            return true;
        }

        public Aisling SendAnimation(ushort animation, Sprite to, Sprite @from, byte speed = 100)
        {
            var format = new ServerFormat29((uint) @from.Serial, (uint) to.Serial, animation, 0, speed);
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

        public void SendAnimation(ushort v, Position position)
        {
            Show(Scope.NearbyAislings, new ServerFormat29(v, position.X, position.Y));
        }

        public Sprite ApplyBuff(string buff)
        {
            if (ServerContextBase.GlobalBuffCache.ContainsKey(buff))
            {
                var Buff = Clone<Buff>(ServerContextBase.GlobalBuffCache[buff]);

                if (Buff == null || string.IsNullOrEmpty(Buff.Name))
                    return null;

                if (!HasBuff(Buff.Name))
                    Buff.OnApplied(this, Buff);
            }

            return this;
        }

        public Sprite ApplyDebuff(string debuff)
        {
            if (ServerContextBase.GlobalDeBuffCache.ContainsKey(debuff))
            {
                var Debuff = Clone<Debuff>(ServerContextBase.GlobalDeBuffCache[debuff]);
                if (!HasDebuff(Debuff.Name))
                    Debuff.OnApplied(this, Debuff);
            }

            return this;
        }


        #endregion
    }
}