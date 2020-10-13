#region

using Darkages.Common;
using Darkages.Network;
using Darkages.Network.Game;
using Darkages.Network.Game.Components;
using Darkages.Network.Object;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using static Darkages.Types.ElementManager;

#endregion

namespace Darkages.Types
{
    public abstract class Sprite : ObjectManager, INotifyPropertyChanged, ISprite
    {
        [JsonIgnore] public bool Abyss;

        [JsonIgnore] public byte LastDirection;

        [JsonIgnore] public Position LastPosition;

        public int X;

        public int Y;

        private static readonly int[][] Directions =
        {
            new[] {+0, -1},
            new[] {+1, +0},
            new[] {+0, +1},
            new[] {-1, +0}
        };

        private readonly Random _rnd = new Random();

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

        public event PropertyChangedEventHandler PropertyChanged;

        [JsonIgnore]
        public static int[][] DirectionTable { get; } =
        {
            new[] {-1, +3, -1},
            new[] {+0, -1, +2},
            new[] {-1, +1, -1}
        };

        public byte _Con { get; set; }
        public byte _Dex { get; set; }
        public byte _Dmg { get; set; }
        public byte _Hit { get; set; }
        public byte _Int { get; set; }
        public int _MaximumHp { get; set; }
        public int _MaximumMp { get; set; }
        public byte _Mr { get; set; }
        public int _Regen { get; set; }
        public byte _Str { get; set; }
        public byte _Wis { get; set; }

        public int Amplified { get; set; }

        [JsonIgnore] public DateTime AbandonedDate { get; set; }

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

        [JsonIgnore] public bool Alive => CurrentHp > 0;
        [JsonIgnore] public bool Attackable => this is Monster || this is Aisling || this is Mundane;
        [JsonIgnore] public int BonusAc { get; set; }
        [JsonIgnore] public int BonusCon { get; set; }
        [JsonIgnore] public int BonusDex { get; set; }
        [JsonIgnore] public byte BonusDmg { get; set; }
        [JsonIgnore] public byte BonusHit { get; set; }
        [JsonIgnore] public int BonusHp { get; set; }
        [JsonIgnore] public int BonusInt { get; set; }
        [JsonIgnore] public int BonusMp { get; set; }
        [JsonIgnore] public byte BonusMr { get; set; }
        [JsonIgnore] public int BonusRegen { get; set; }
        [JsonIgnore] public int BonusStr { get; set; }
        [JsonIgnore] public int BonusWis { get; set; }
        [JsonIgnore] public bool CanCast => !(IsFrozen || IsSleeping);
        [JsonIgnore] public bool CanMove => !(IsFrozen || IsSleeping || IsParalyzed);
        [JsonIgnore] public GameClient Client { get; set; }

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
                var tmp = (byte)(_Dex + BonusDex).Clamp(1, byte.MaxValue);
                return tmp > 255 ? (byte)255 : tmp;
            }
        }


        [JsonIgnore]
        public byte Int
        {
            get
            {
                var tmp = (byte)(_Int + BonusInt).Clamp(1, byte.MaxValue);
                return tmp > 255 ? (byte)255 : tmp;
            }
        }

        [JsonIgnore]
        public byte Str
        {
            get
            {
                var tmp = (byte)(_Str + BonusStr).Clamp(1, byte.MaxValue);
                return tmp > 255 ? (byte)255 : tmp;
            }
        }


        [JsonIgnore]
        public byte Wis
        {
            get
            {
                var tmp = (byte)(_Wis + BonusWis).Clamp(1, byte.MaxValue);
                return tmp > 255 ? (byte)255 : tmp;
            }
        }

        public int CurrentHp { get; set; }
        public int CurrentMapId { get; set; }
        public int CurrentMp { get; set; }

        public ConcurrentDictionary<string, Buff> Buffs { get; set; }
        public ConcurrentDictionary<string, Debuff> Debuffs { get; set; }

        public Element DefenseElement { get; set; }


        public byte Direction { get; set; }
        [JsonIgnore] public byte Dmg => (byte) (_Dmg + BonusDmg).Clamp(0, byte.MaxValue);
        [JsonIgnore] public bool EmpoweredAssail { get; set; }
        [JsonIgnore] public TileContent EntityType { get; set; }
        [JsonIgnore] public int GroupId { get; set; }
        [JsonIgnore] public byte Hit => (byte) (_Hit + BonusHit).Clamp(0, byte.MaxValue);

        public bool Immunity { get; set; }

        [JsonIgnore] public bool IsAited => HasBuff("aite");

        [JsonIgnore] public bool IsBleeding => HasDebuff("bleeding");

        [JsonIgnore] public bool IsBlind => HasDebuff("blind");

        [JsonIgnore] public bool IsConfused => HasDebuff("confused");

        [JsonIgnore] public bool IsCursed => HasDebuff(i => i.Name.ToLower().Contains("cradh"));

        [JsonIgnore] public bool IsFrozen => HasDebuff("frozen");

        [JsonIgnore]
        public bool IsParalyzed => HasDebuff("paralyze") || HasDebuff(i => i.Name.ToLower().Contains("beag suain"));

        [JsonIgnore] public bool IsPoisoned => HasDebuff(i => i.Name.ToLower().Contains("puinsein"));

        [JsonIgnore] public bool IsSleeping => HasDebuff("sleep");

        [JsonIgnore] public DateTime LastMenuInvoked { get; set; } = DateTime.UtcNow;

        [JsonIgnore] public DateTime LastMovementChanged { get; set; }

        [JsonIgnore] public DateTime LastTargetAcquired { get; set; }

        public DateTime LastTurnUpdated { get; set; }

        [JsonIgnore] public DateTime LastUpdated { get; set; }

        public PrimaryStat MajorAttribute { get; set; }

        [JsonIgnore]
        public Area Map => ServerContext.GlobalMapCache.ContainsKey(CurrentMapId)
            ? ServerContext.GlobalMapCache[CurrentMapId]
            : null;

        [JsonIgnore] public int MaximumHp => _MaximumHp + BonusHp;
        [JsonIgnore] public int MaximumMp => _MaximumMp + BonusMp;
        [JsonIgnore] public byte Mr => (byte) (_Mr + BonusMr).Clamp(0, 70);
        public Element OffenseElement { get; set; }
        [JsonIgnore] public int PendingX { get; set; }
        [JsonIgnore] public int PendingY { get; set; }
        [JsonIgnore] public Position Position => new Position(XPos, YPos);
        [JsonIgnore] public int Regen => (_Regen + BonusRegen).Clamp(0, 300);
        public int Serial { get; set; }
        public bool SpellReflect { get; set; }

        [JsonIgnore] public Sprite Target { get; set; }

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

        [JsonIgnore]
        public int Level => EntityType == TileContent.Aisling ? ((Aisling) this).ExpLevel
            : EntityType == TileContent.Monster ? ((Monster) this).Template.Level
            : EntityType == TileContent.Mundane ? ((Mundane) this).Template.Level
            : EntityType == TileContent.Item ? ((Item) this).Template.LevelRequired : 0;

        public static Element CheckRandomElement(Element element)
        {
            if (element == Element.Random)
                element = Generator.RandomEnumValue<Element>();

            return element;
        }

        public static float Sqrt(float number)
        {
            ulong i;
            var x = number * 0.5f;
            var y = number;

            unsafe
            {
                i = *(ulong*) &y;
                i = 0x5F3759DF - (i >> 1);
                y = *(float*) &i;
                y *= 1.5f - x * y * y;
                y *= 1.5f - x * y * y;
            }

            return number * y;
        }

        public Aisling Aisling(Sprite obj)
        {
            if (obj is Aisling aisling)
                return aisling;

            return null;
        }

        public Aisling[] AislingsNearby()
        {
            return GetObjects<Aisling>(Map, i => i != null && i.WithinRangeOf(this)).ToArray();
        }

        public void Animate(ushort animation, byte speed = 100)
        {
            Show(Scope.NearbyAislings, new ServerFormat29((uint) Serial, (uint) Serial, animation, animation, speed));
        }

        public Sprite ApplyBuff(string buffName)
        {
            if (ServerContext.GlobalBuffCache.ContainsKey(buffName))
            {
                var buff = Clone<Buff>(ServerContext.GlobalBuffCache[buffName]);

                if (buff == null || string.IsNullOrEmpty(buff.Name))
                    return null;

                if (!HasBuff(buff.Name))
                    buff.OnApplied(this, buff);
            }

            return this;
        }

        public void ApplyDamage(Sprite source, int dmg, Element element, byte sound = 1)
        {
            element = CheckRandomElement(element);

            var saved = source.OffenseElement;
            {
                source.OffenseElement = element;
                ApplyDamage(source, dmg, sound);
                source.OffenseElement = saved;
            }
        }

        public void ApplyDamage(Sprite damageDealingSprite, int dmg, byte sound = 1,
            Action<int> dmgcb = null, bool forceTarget = false)
        {

            int ApplyPVPMod()
            {
                if (Map.Flags.HasFlag(MapFlags.PlayerKill))
                    return dmg = (int) (dmg * 0.75);

                return dmg;
            }

            int ApplyBehindTargetMod()
            {
                if (damageDealingSprite is Aisling aisling)
                    if (aisling.Client.IsBehind(this))
                        dmg += (int) ((dmg + ServerContext.Config.BehindDamageMod) / 1.99);
                return dmg;
            }

            if (!WithinRangeOf(damageDealingSprite))
                return;

            if (!Attackable)
                return;

            if (!CanBeAttackedHere(damageDealingSprite))
                return;

            dmg = ApplyPVPMod(); 
            dmg = ApplyBehindTargetMod();
            dmg = ApplyWeaponBonuses(damageDealingSprite, dmg);

            if (dmg > 0)
                ApplyEquipmentDurability(dmg);

            if (!DamageTarget(damageDealingSprite, ref dmg, sound, dmgcb, forceTarget))
                return;

            {
                if (damageDealingSprite is Aisling aisling)
                    if (aisling.GameMaster)
                        dmg *= 200;
            }

            OnDamaged(damageDealingSprite, dmg);
        }

        public Sprite ApplyDebuff(string debuffName)
        {
            if (ServerContext.GlobalDeBuffCache.ContainsKey(debuffName))
            {
                var debuff = Clone<Debuff>(ServerContext.GlobalDeBuffCache[debuffName]);
                if (!HasDebuff(debuff.Name))
                    debuff.OnApplied(this, debuff);
            }

            return this;
        }

        public void ApplyEquipmentDurability(int dmg)
        {
            if (this is Aisling aisling && aisling.DamageCounter++ % 2 == 0 && dmg > 0)
                aisling.EquipmentManager.DecreaseDurability();
        }

        public int ApplyWeaponBonuses(Sprite source, int dmg)
        {
            if (!(source is Aisling aisling))
                return dmg;

            if (aisling.EquipmentManager != null && (aisling.EquipmentManager.Weapon?.Item == null || aisling.Weapon <= 0))
                return dmg;

            if (aisling.EquipmentManager != null)
            {
                var weapon = aisling.EquipmentManager.Weapon.Item;

                lock (Generator.Random)
                {
                    dmg += Generator.Random.Next(
                        weapon.Template.DmgMin + aisling.BonusDmg * 1,
                        weapon.Template.DmgMax + aisling.BonusDmg * 5);
                }
            }

            return dmg;
        }

        public double CalculateElementalDamageMod(Element element)
        {
            var script = ScriptManager.Load<ElementFormulaScript>(ServerContext.Config.ElementTableScript, this);

            return script?.Values.Sum(s => s.Calculate(this, element)) ?? 0.0;
        }

        public bool CanBeAttackedHere(Sprite source)
        {
            if (!(source is Aisling) || !(this is Aisling))
                return true;

            if (CurrentMapId <= 0 || !ServerContext.GlobalMapCache.ContainsKey(CurrentMapId))
                return true;

            return ServerContext.GlobalMapCache[CurrentMapId].Flags.HasFlag(MapFlags.PlayerKill);
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
                        canTag = attackingPlayer.GroupId == taggeduser.GroupId;
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

        public bool CanUpdate()
        {
            if (IsSleeping || IsFrozen || IsBlind)
                return false;

            if (this is Monster || this is Mundane)
                if (CurrentHp == 0)
                    return false;

            if (ServerContext.Config.CanMoveDuringReap)
                return true;

            if (!(this is Aisling aisling))
                return true;

            if (!aisling.Skulled)
                return true;

            aisling.Client.SystemMessage(ServerContext.Config.ReapMessageDuringAction);
            return false;
        }

        public TSprite Cast<TSprite>()
            where TSprite : Sprite
        {
            return this as TSprite;
        }

        public int CompleteDamageApplication(int dmg, byte sound, Action<int> dmgcb, double amplifier)
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
                Sound = sound
            };

            Show(Scope.VeryNearbyAislings, hpbar);
            {
                dmgcb?.Invoke(dmgApplied);
            }

            return dmgApplied;
        }

        public bool DamageTarget(Sprite damageDealingSprite,
            ref int dmg, byte sound,
            Action<int> dmgcb, bool forced)
        {
            if (this is Monster)
            {
                if (damageDealingSprite is Aisling aisling)
                    if (!CanTag(aisling, forced))
                    {
                        aisling.Client.SendMessage(0x02, ServerContext.Config.CantAttack);
                        return false;
                    }
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
            {
                if (ServerContext.Config.SleepProcsDoubleDmg)
                {
                    dmg <<= 1;
                }

                RemoveDebuff("sleep");
            }

            if (IsAited && dmg > 5)
                dmg /= ServerContext.Config.AiteDamageReductionMod;

            var amplifier = GetElementalModifier(damageDealingSprite);
            {
                dmg = ComputeDmgFromAc(dmg);
                dmg = CompleteDamageApplication(dmg, sound, dmgcb, amplifier);
            }

            return true;
        }

        public int DistanceFrom(int x, int y)
        {
            return Math.Abs(X - x) + Math.Abs(Y - y);
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

        public int GetBaseDamage(Sprite target, MonsterDamageType type)
        {
            var script = ScriptManager.Load<DamageFormulaScript>(ServerContext.Config.BaseDamageScript, this);
            return script?.Values.Sum(s => s.Calculate(this, target, type)) ?? 1;
        }

        public string GetDebuffName(Func<Debuff, bool> p)
        {
            if (Debuffs == null || Debuffs.Count == 0)
                return string.Empty;

            return Debuffs.Select(i => i.Value)
                .FirstOrDefault(p)
                ?.Name;
        }

        public double GetElementalModifier(Sprite damageDealingSprite)
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
                ? ServerContext.Config.FasNadurStrength + 10
                : ServerContext.Config.MorFasNadurStrength + 30;

            return amplifier;
        }

        public List<Sprite> GetInfront(Sprite sprite, int tileCount = 1)
        {
            return GetInfront(tileCount).Where(i => i != null && i.Serial != sprite.Serial).ToList();
        }

        public List<Sprite> GetInfront(int tileCount = 1, bool intersect = false)
        {
            return GetInfront(tileCount).ToList();
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

        public IEnumerable<Sprite> GetSprites(int x, int y)
        {
            return GetObjects(Map, i => i.XPos == x && i.YPos == y, Get.All);
        }

        public void GiveHP(int value)
        {
            _MaximumHp += value;
        }

        public void GiveMP(int value)
        {
            _MaximumMp += value;
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

        public void HideFrom(Aisling nearbyAisling)
        {
            if (nearbyAisling != null)
                nearbyAisling.Show(Scope.Self, new ServerFormat0E(Serial));
        }

        public void Kill()
        {
            CurrentHp = 0;
        }

        public Monster Monster(Sprite obj)
        {
            if (obj is Monster monster)
                return monster;

            return null;
        }

        public Monster[] MonstersNearby()
        {
            return GetObjects<Monster>(Map, i => i != null && i.WithinRangeOf(this)).ToArray();
        }

        public Mundane[] MundanesNearby()
        {
            return GetObjects<Mundane>(Map, i => i != null && i.WithinRangeOf(this)).ToArray();
        }

        public bool NextTo(int x, int y)
        {
            var xDist = Math.Abs(x - X);
            var yDist = Math.Abs(y - Y);

            return xDist + yDist == 1;
        }

        public void OnDamaged(Sprite source, int dmg)
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

        public void Remove()
        {
            var nearby = GetObjects<Aisling>(null, i => i != null && i.LoggedIn);
            var response = new ServerFormat0E(Serial);

            foreach (var o in nearby)
                for (var i = 0; i < 2; i++)
                    o?.Client?.Send(response);

            DeleteObject();
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

        public void RemoveBuffsAndDebuffs()
        {
            RemoveAllBuffs();
            RemoveAllDebuffs();
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

        public Aisling SendAnimation(ushort animation, Sprite to, Sprite from, byte speed = 100)
        {
            var format = new ServerFormat29((uint) from.Serial, (uint) to.Serial, animation, 0, speed);
            {
                Show(Scope.NearbyAislings, format);
            }

            return Aisling(this);
        }

        public void SendAnimation(ushort v, Position position)
        {
            Show(Scope.NearbyAislings, new ServerFormat29(v, position.X, position.Y));
        }

        public void Shout(string message)
        {
            var response = new ServerFormat0D
            {
                Serial = Serial,
                Type = 0x01,
                Text = message
            };

            Show(Scope.AislingsOnSameMap, response);
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
                    foreach (var gc in GetObjects<Aisling>(Map, that => that != null && WithinRangeOf(that)))
                        if (gc != null && gc.Serial != Serial)
                            if (gc.Client != null)
                                if (format != null)
                                    gc.Client.Send(format);
                }
                else if (op == Scope.NearbyAislings)
                {
                    foreach (var gc in GetObjects<Aisling>(Map, that => WithinRangeOf(that)))
                    {
                        gc?.Client.Send(format);
                    }
                }
                else if (op == Scope.Clan)
                {
                    foreach (var gc in GetObjects<Aisling>(null,
                        that => that != null &&
                                !that.Abyss &&
                                !string.IsNullOrEmpty(that.Clan) &&
                                string.Equals(that.Clan, Aisling(this).Clan, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        gc?.Client.Send(format);
                    }
                }

                else if (op == Scope.VeryNearbyAislings)
                {
                    foreach (var gc in GetObjects<Aisling>(Map, that =>
                        WithinRangeOf(that, ServerContext.Config.VeryNearByProximity)))
                        gc.Client.Send(format);
                }
                else if (op == Scope.AislingsOnSameMap)
                {
                    foreach (var gc in GetObjects<Aisling>(Map, that => CurrentMapId == that.CurrentMapId))
                        if (gc != null)
                            gc.Client.Send(format);
                }
                else if (op == Scope.GroupMembers)
                {
                    if (!(this is Aisling))
                        return;

                    foreach (var gc in GetObjects<Aisling>(Map, that => ((Aisling) this).GroupParty.Has(that)))
                        if (gc != null)
                            gc.Client.Send(format);
                }
                else if (op == Scope.NearbyGroupMembersExcludingSelf)
                {
                    if (!(this is Aisling))
                        return;

                    foreach (var gc in GetObjects<Aisling>(Map,
                        that => that.WithinRangeOf(this) && ((Aisling) this).GroupParty.Has(that)))
                        if (gc != null)
                            gc.Client.Send(format);
                }
                else if (op == Scope.NearbyGroupMembers)
                {
                    if (!(this is Aisling))
                        return;

                    foreach (var gc in GetObjects<Aisling>(Map,
                        that => that.WithinRangeOf(this) && ((Aisling) this).GroupParty.Has(that)))
                        if (gc != null)
                            gc.Client.Send(format);
                }
                else if (op == Scope.DefinedAislings)
                {
                    if (definer == null)
                        return;

                    foreach (var gc in definer)
                        if (gc != null)
                            (gc as Aisling)?.Client.Send(format);
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

        public bool TrapsAreNearby()
        {
            return Trap.Traps.Select(i => i.Value).Any(i => i.CurrentMapId == CurrentMapId);
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

        public void Update()
        {
            Show(Scope.NearbyAislings, new ServerFormat0E(Serial));
            Show(Scope.NearbyAislings, new ServerFormat07(new[] {this}));
        }

        public void UpdateBuffs(TimeSpan elapsedTime)
        {
            foreach (var (_, buff) in Buffs) buff.Update(this, elapsedTime);
        }

        public void UpdateDebuffs(TimeSpan elapsedTime)
        {
            foreach (var (_, debuff) in Debuffs) debuff.Update(this, elapsedTime);
        }

        public void UseSkillScript(string skillName)
        {
            if (!ServerContext.GlobalSkillTemplateCache.ContainsKey(skillName))
                return;

            var spellTemplate = ServerContext.GlobalSkillTemplateCache[skillName];
            var skill = Skill.Create(1, spellTemplate);

            foreach (var script in skill.Scripts.Values) script?.OnUse(this);
        }

        public void UseSpellScript(string spellName, Sprite target)
        {
            if (!ServerContext.GlobalSpellTemplateCache.ContainsKey(spellName))
                return;

            var scripts = ScriptManager.Load<SpellScript>(spellName,
                Spell.Create(1, ServerContext.GlobalSpellTemplateCache[spellName]));

            foreach (var script in scripts.Values) script?.OnUse(this, target);
        }

        public virtual bool Walk()
        {
            void Step(int i, int savedY1)
            {
                var response = new ServerFormat0C
                {
                    Direction = Direction,
                    Serial = Serial,
                    X = (short) i,
                    Y = (short) savedY1
                };

                X = PendingX;
                Y = PendingY;

                Show(Scope.NearbyAislingsExludingSelf, response);
                {
                    LastMovementChanged = DateTime.UtcNow;
                    LastPosition = new Position(i, savedY1);
                }
            }

            //update all objects nearby before we take a step.
            foreach (var obj in AislingsNearby())
                ObjectComponent.UpdateClientObjects(obj);

            var savedX = X;
            var savedY = Y;

            PendingX = X;
            PendingY = Y;

            var allowGhostWalk = false;

            //only gms can ghost walk, and only aislings can be gms.
            if (this is Aisling aisling)
                if (aisling.GameMaster)
                    allowGhostWalk = true;

            //check position before we take a step.
            if (!allowGhostWalk && this is Aisling)
            {
                if (Map?.IsWall(savedX, savedY) ?? false)
                    return false;

                if (!Map?.ObjectGrid[savedX, savedY].IsPassable(this, this is Aisling) ?? false)
                    return false;
            }

            if (Direction == 0)
                PendingY--;
            else if (Direction == 1)
                PendingX++;
            else if (Direction == 2)
                PendingY++;
            else if (Direction == 3)
                PendingX--;

            //check position after we take a step.
            if (!allowGhostWalk && this is Aisling)
            {
                if (Map != null && Map.IsWall(PendingX, PendingY))
                    return false;

                if (Map != null && !Map.ObjectGrid[PendingX, PendingY].IsPassable(this, this is Aisling))
                    return false;
            }

            //commit.
            Step(savedX, savedY);

            //reset our PendingX, PendingY back to our previous step.
            PendingX = savedX;
            PendingY = savedY;

            //update all objects nearby after we take a step.
            foreach (var obj in AislingsNearby())
                ObjectComponent.UpdateClientObjects(obj);

            return true;
        }

        public bool WalkTo(int x, int y)
        {
            return WalkTo(x, y, false);
        }

        public bool WalkTo(int x, int y, bool ignoreWalls = false)
        {
            var buffer = new byte[2];
            var length = float.PositiveInfinity;
            var offset = 0;

            for (byte i = 0; i < 4; i++)
            {
                var newX = X + Directions[i][0];
                var newY = Y + Directions[i][1];

                if (newX == x &&
                    newY == y)
                    return false;

                if (Map.IsWall(newX, newY))
                    continue;

                if (GetObjects(Map, n => n.Serial == Serial && n.X == newX && n.Y == newY,
                    Get.Monsters | Get.Aislings | Get.Mundanes).Any())
                    continue;

                var xDist = x - newX;
                var yDist = y - newY;
                var tDist = Sqrt(xDist * xDist + yDist * yDist);

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
                return false;

            lock (Generator.Random)
            {
                var pendingDirection = buffer[Generator.Random.Next(0, offset) % buffer.Length];
                Direction = pendingDirection;

                return Walk();
            }
        }

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

        public bool WithinRangeOf(Sprite other, bool checkMap = true)
        {
            return other != null && WithinRangeOf(other, ServerContext.Config.WithinRangeProximity, checkMap);
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

        private int ComputeDmgFromAc(int dmg)
        {
            var script = ScriptManager.Load<FormulaScript>(ServerContext.Config.ACFormulaScript, this);

            if (script == null)
                return dmg;

            return script.Aggregate(dmg, (current, s) => s.Value.Calculate(this, current));
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

        private List<Sprite> GetInfront(int tileCount = 1)
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

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Identification & Position

        #endregion

        #region Attributes

        #endregion

        #region Status

        #endregion

        #region Sprite Methods

        #endregion
    }
}