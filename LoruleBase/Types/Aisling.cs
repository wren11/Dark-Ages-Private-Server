#region

using Darkages.Common;
using Darkages.Network;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

#endregion

namespace Darkages
{
    public class Aisling : Sprite
    {
        public Dictionary<string, EphemeralReactor> ActiveReactors = new Dictionary<string, EphemeralReactor>();

        [JsonIgnore] public int DamageCounter = 0;

        public Dictionary<string, int> MonsterKillCounters = new Dictionary<string, int>();

        [JsonIgnore] public List<Popup> Popups = new List<Popup>();

        public Dictionary<string, DateTime> Reactions = new Dictionary<string, DateTime>();

        [JsonIgnore] public HashSet<Sprite> View = new HashSet<Sprite>();
        private readonly object syncLock = new object();

        public Aisling()
        {
            OffenseElement = ElementManager.Element.None;
            DefenseElement = ElementManager.Element.None;
            Clan = string.Empty;
            LegendBook = new Legend();
            ClanTitle = string.Empty;
            ClanRank = string.Empty;
            ActiveSpellInfo = null;
            LoggedIn = false;
            ActiveStatus = ActivityStatus.Awake;
            PortalSession = new PortalSession();
            Quests = new List<Quest>();
            PartyStatus = GroupStatus.AcceptingRequests;
            Remains = new CursedSachel(this);
            ActiveReactor = null;
            DiscoveredMaps = new List<int>();
            Popups = new List<Popup>();
            GroupId = 0;
        }

        public int AbpLevel { get; set; }
        public int AbpNext { get; set; }
        public int AbpTotal { get; set; }
        [JsonIgnore] public Reactor ActiveReactor { get; set; }
        [JsonIgnore] public DialogSequence ActiveSequence { get; set; }
        [JsonIgnore] public CastInfo ActiveSpellInfo { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ActivityStatus ActiveStatus { get; set; }

        public AnimalForm AnimalForm { get; set; }
        [JsonIgnore] public int AreaId => CurrentMapId;
        public ushort Armor { get; set; }
        public Bank BankManager { get; set; }
        public byte Blind { get; set; }
        public int BodyColor { get; set; }
        public int BodyStyle { get; set; }
        public byte BootColor { get; set; }
        public byte Boots { get; set; }
        [JsonIgnore] public bool CanReact { get; set; }
        public string Clan { get; set; }
        public string ClanRank { get; set; }
        public string ClanTitle { get; set; }
        public DateTime Created { get; set; }
        public int CurrentWeight { get; set; }

        [JsonIgnore] public bool Dead => IsDead();

        public List<int> DiscoveredMaps { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public BodySprite Display { get; set; }

        public EquipmentManager EquipmentManager { get; set; }
        [JsonIgnore] public ExchangeSession Exchange { get; set; }

        public int ExpLevel { get; set; }
        public uint ExpNext { get; set; }
        public uint ExpTotal { get; set; }
        public int FaceColor { get; set; }
        public int FaceStyle { get; set; }
        public AislingFlags Flags { get; internal set; }
        public bool GameMaster { get; set; }
        public int GamePoints { get; set; }

        public List<ClientGameSettings> GameSettings { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Gender Gender { get; set; }

        public int GoldPoints { get; set; }

        [JsonIgnore]
        public Party GroupParty
        {
            get
            {
                if (ServerContextBase.GlobalGroupCache.ContainsKey(GroupId))
                    return ServerContextBase.GlobalGroupCache[GroupId];

                return null;
            }
        }

        public byte HairColor { get; set; }
        public byte HairStyle { get; set; }
        public byte HeadAccessory1 { get; set; }
        public byte HeadAccessory2 { get; set; }
        public int Helmet { get; set; }
        public Inventory Inventory { get; set; }
        public bool Invisible { get; set; }
        [JsonIgnore] public bool IsCastingSpell { get; set; }
        public DateTime LastLogged { get; set; }
        [JsonIgnore] public int LastMapId { get; set; }

        [JsonIgnore]
        public bool LeaderPrivileges
        {
            get
            {
                if (!ServerContextBase.GlobalGroupCache.ContainsKey(GroupId))
                    return false;

                var group = ServerContextBase.GlobalGroupCache[GroupId];
                return group != null && group.LeaderName.ToLower() == Username.ToLower();
            }
        }

        public Legend LegendBook { get; set; }
        public bool LoggedIn { get; set; }

        [JsonIgnore]
        public int MaximumWeight => (int)(ExpLevel / 4 + _Str + ServerContextBase.Config.WeightIncreaseModifer);

        public ushort MonsterForm { get; set; } = 0;
        public byte NameColor { get; set; }
        public string Nation { get; set; } = "Mileth"; // default nation.
        public byte OverCoat { get; set; }
        public byte Pants { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public GroupStatus PartyStatus { get; set; }

        public string Password { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Class Path { get; set; }

        public byte[] PictureData { get; set; }

        [JsonIgnore]
        public NationTemplate PlayerNation
        {
            get
            {
                if (Nation != null) return ServerContextBase.GlobalNationTemplateCache[Nation];
                throw new InvalidOperationException();
            }
        }

        [JsonIgnore] public PortalSession PortalSession { get; set; }
        [JsonIgnore] [Browsable(false)] public new Position Position => new Position(XPos, YPos);
        public string ProfileMessage { get; set; }
        [JsonIgnore] public bool ProfileOpen { get; set; }
        public List<Quest> Quests { get; set; }
        [JsonIgnore] public bool ReactorActive { get; set; }
        public CursedSachel Remains { get; set; }
        public byte Resting { get; set; }
        public byte Shield { get; set; }
        public SkillBook SkillBook { get; set; }
        [JsonIgnore] public bool Skulled => HasDebuff("skulled");

        public SpellBook SpellBook { get; set; }

        public ClassStage Stage { get; set; }
        public int StatPoints { get; set; }
        public int Title { get; set; }
        public bool TutorialCompleted { get; set; }
        public string Username { get; set; }
        [JsonIgnore] public bool UsingTwoHanded { get; set; }
        public ushort Weapon { get; set; }
        public int World { get; set; } = 2;
        [JsonIgnore] public List<Aisling> PartyMembers => GroupParty?.PartyMembers;

        public static Aisling Create()
        {
            var result = new Aisling
            {
                Username = string.Empty,
                Password = string.Empty,
                AbpLevel = 0,
                AbpTotal = 0,
                AbpNext = 0,
                ExpLevel = 1,
                ExpTotal = 1,
                ExpNext = 600,
                Gender = 0,
                Title = 0,
                CurrentMapId = ServerContextBase.Config.StartingMap,
                Stage = ClassStage.Class,
                Path = Class.Peasant,
                CurrentHp = 60,
                CurrentMp = 30,
                _MaximumHp = 60,
                _MaximumMp = 30,
                _Str = 3,
                _Int = 3,
                _Wis = 3,
                _Con = 3,
                _Dex = 3,
                GamePoints = 0,
                GoldPoints = 0,
                StatPoints = 0,
                BodyColor = 0,
                BodyStyle = 0,
                FaceColor = 0,
                FaceStyle = 0,
                HairColor = 0,
                HairStyle = 0,
                NameColor = 1,
                BootColor = 0,
                Amplified = 0,
                TutorialCompleted = false,
                SkillBook = new SkillBook(),
                SpellBook = new SpellBook(),
                Inventory = new Inventory(),
                BankManager = new Bank(),
                Created = DateTime.UtcNow,
                LastLogged = DateTime.UtcNow,
                XPos = ServerContextBase.Config.StartingPosition.X,
                YPos = ServerContextBase.Config.StartingPosition.Y,
                AnimalForm = AnimalForm.None,
                Nation = "Mileth",
                EquipmentManager = new EquipmentManager(null),
            };

            if (ServerContextBase.Config.GiveAssailOnCreate)
                Skill.GiveTo(result, "Assail", 1);

            if (ServerContextBase.Config.DevMode)
            {
                foreach (var temp in ServerContextBase.GlobalSpellTemplateCache)
                    Spell.GiveTo(result, temp.Value.Name);

                foreach (var temp in ServerContextBase.GlobalSkillTemplateCache)
                    Skill.GiveTo(result, temp.Value.Name);
            }

            return result;
        }

        public bool AcceptQuest(Quest lpQuest)
        {
            lock (syncLock)
            {
                if (Quests.All(i => i.Name != lpQuest.Name))
                {
                    Quests.Add(lpQuest);

                    return true;
                }
            }

            return false;
        }

        public void Assail()
        {
            if (Client != null) GameServer.ActivateAssails(Client);
        }

        public void CancelExchange()
        {
            if (Exchange == null || Exchange.Trader == null)
                return;

            var trader = Exchange.Trader;

            var exchangeA = Exchange;
            var exchangeB = trader.Exchange;

            var itemsA = exchangeA.Items.ToArray();
            var itemsB = exchangeB.Items.ToArray();

            var goldA = exchangeA.Gold;
            var goldB = exchangeB.Gold;

            Exchange = null;
            trader.Exchange = null;

            foreach (var item in itemsB)
                if (item.GiveTo(trader))
                {
                }

            foreach (var item in itemsA)
                if (item.GiveTo(this))
                {
                }

            GoldPoints += goldA;
            trader.GoldPoints += goldB;

            if (trader.GoldPoints > ServerContextBase.Config.MaxCarryGold)
                trader.GoldPoints = ServerContextBase.Config.MaxCarryGold;
            if (GoldPoints > ServerContextBase.Config.MaxCarryGold)
                GoldPoints = ServerContextBase.Config.MaxCarryGold;

            trader.Client.SendStats(StatusFlags.StructC);
            Client.SendStats(StatusFlags.StructC);

            var packet = new NetworkPacketWriter(Client);
            packet.Write((byte)0x42);
            packet.Write((byte)0x00);

            packet.Write((byte)0x04);
            packet.Write((byte)0x00);
            packet.WriteStringA("Trade was aborted.");
            Client.Send(packet);

            packet = new NetworkPacketWriter(Client);
            packet.Write((byte)0x42);
            packet.Write((byte)0x00);

            packet.Write((byte)0x04);
            packet.Write((byte)0x01);
            packet.WriteStringA("Trade was aborted.");
            trader.Client.Send(packet);
        }

        public bool CanSeeGhosts()
        {
            return IsDead();
        }

        public Aisling Cast(Spell spell, Sprite target, byte actionSpeed = 30)
        {
            var action = new ServerFormat1A
            {
                Serial = Serial,
                Number = (byte)(Path == Class.Priest ? 0x80 : Path == Class.Wizard ? 0x88 : 0x06),
                Speed = actionSpeed
            };

            if (target != null)
            {
                if (Aisling(target)?.SendAnimation(spell.Template.Animation, target, this) == null)
                    SendAnimation(spell.Template.Animation, target, this);

                if (target.CurrentHp > 0)
                {
                    var hpbar = new ServerFormat13
                    {
                        Serial = target.Serial,
                        Health = (ushort)(100 * target.CurrentHp / target.MaximumHp),
                        Sound = spell.Template.Sound
                    };

                    target.Show(Scope.NearbyAislings, hpbar);
                }

                Show(Scope.NearbyAislings, action);
                CurrentMp -= spell.Template.ManaCost;

                if (CurrentMp < 0)
                    CurrentMp = 0;

                Client.SendMessage(0x02, "you cast " + spell.Template.Name + ".");
            }

            return this;
        }

        public bool CastDeath()
        {
            if (!Client.Aisling.Flags.HasFlag(AislingFlags.Ghost))
            {
                LastMapId = CurrentMapId;
                LastPosition = Position;

                Client.CloseDialog();
                Client.AislingToGhostForm();

                return true;
            }

            return false;
        }

        public void CastSpell(Spell spell)
        {
            if (!spell.CanUse())
            {
                spell.InUse = false;
                return;
            }

            if (spell.InUse)
                return;

            var info = Client.Aisling.ActiveSpellInfo;

            if (info != null)
            {
                if (!string.IsNullOrEmpty(info.Data))
                    foreach (var script in spell.Scripts.Values)
                        script.Arguments = info.Data;

                var target = GetObject(Map, i => i.Serial == info.Target, Get.Monsters | Get.Aislings | Get.Mundanes);
                spell.InUse = true;

                if (spell.Scripts != null)
                {
                    if (target != null)
                    {
                        {
                            if (target is Aisling obj && obj.Serial == info.Target)
                            {
                                foreach (var script in spell.Scripts.Values)
                                    script.OnUse(this, target as Aisling);
                            }
                        }

                        {
                            if (target is Monster obj && obj.Serial == info.Target)
                                foreach (var script in spell.Scripts.Values)
                                    script.OnUse(this, target as Monster);
                        }

                        {
                            if (target is Mundane obj && obj.Serial == info.Target)
                                foreach (var script in spell.Scripts.Values)
                                    script.OnUse(this, target as Mundane);
                        }
                    }
                    else
                    {
                        foreach (var script in spell.Scripts.Values)
                            script.OnUse(this, this);
                    }
                }
            }

            spell.NextAvailableUse = DateTime.UtcNow.AddSeconds(info.SpellLines > 0 ? 1 : 0.2);
            spell.InUse = false;

            if (spell.Template.Cooldown > 0)
                Client.Send(new ServerFormat3F(0,
                    spell.Slot,
                    spell.Template.Cooldown));
        }

        public void CompleteQuest(string lpName)
        {
            var obj = Quests.Find(i => i.Name == lpName);
            obj.Completed = true;
        }

        public void DestroyReactor(Reactor Actor)
        {
            if (Reactions.ContainsKey(Actor.Name))
                Reactions.Remove(Actor.Name);

            ActiveReactor = null;
            ReactorActive = false;
        }

        public void FinishExchange()
        {
            var trader = Exchange.Trader;
            var exchangeA = Exchange;
            var exchangeB = trader.Exchange;
            var itemsA = exchangeA.Items.ToArray();
            var itemsB = exchangeB.Items.ToArray();
            var goldA = exchangeA.Gold;
            var goldB = exchangeB.Gold;

            Exchange = null;
            trader.Exchange = null;

            foreach (var item in itemsB)
                if (item.GiveTo(this))
                {
                }

            foreach (var item in itemsA)
                if (item.GiveTo(trader))
                {
                }

            GoldPoints += goldB;
            trader.GoldPoints += goldA;

            if (trader.GoldPoints > ServerContextBase.Config.MaxCarryGold)
                trader.GoldPoints = ServerContextBase.Config.MaxCarryGold;
            if (GoldPoints > ServerContextBase.Config.MaxCarryGold)
                GoldPoints = ServerContextBase.Config.MaxCarryGold;

            exchangeA.Items.Clear();
            exchangeB.Items.Clear();

            trader.Client?.SendStats(StatusFlags.All);
            Client?.SendStats(StatusFlags.All);
        }

        public Skill[] GetAssails()
        {
            return SkillBook.Get(i => i != null && i.Template != null
                                                && i.Template.Type == SkillScope.Assail).ToArray();
        }

        public Quest GetQuest(string name)
        {
            return Quests.FirstOrDefault(i => i.Name == name);
        }

        public bool GiveGold(int offer, bool SendClientUpdate = true)
        {
            if (GoldPoints + offer < ServerContextBase.Config.MaxCarryGold)
            {
                GoldPoints += offer;
                return true;
            }

            if (SendClientUpdate) Client?.SendStats(StatusFlags.StructC);

            return false;
        }

        public Aisling GiveHealth(Sprite target, int value)
        {
            target.CurrentHp += value;

            if (target.CurrentHp > target.MaximumHp) target.CurrentHp = target.MaximumHp;

            return this;
        }

        public void GoHome()
        {
            Client.CloseDialog();
            Client.LeaveArea(true, true);

            if (string.IsNullOrEmpty(Client.Aisling.Nation))
            {
                var destinationMap = ServerContextBase.Config.TransitionZone;

                if (ServerContextBase.GlobalMapCache.ContainsKey(destinationMap))
                {
                    Client.Aisling.XPos = ServerContextBase.Config.TransitionPointX;
                    Client.Aisling.YPos = ServerContextBase.Config.TransitionPointY;
                    Client.Aisling.CurrentMapId = destinationMap;
                }
            }
            else
            {
                if (PlayerNation != null)
                {
                    Client.Aisling.XPos = PlayerNation.MapPosition.X;
                    Client.Aisling.YPos = PlayerNation.MapPosition.Y;
                    Client.Aisling.CurrentMapId = PlayerNation.AreaId;
                }
            }

            Client.EnterArea();
            Client.Refresh();
        }

        public bool HasCompletedQuest(string lpName)
        {
            return Quests.Any(i => i.Name == lpName && i.Completed);
        }

        public bool HasInInventory(string item, int count, out int found)
        {
            var template = ServerContextBase.GlobalItemTemplateCache[item];
            found = 0;

            if (!ServerContextBase.GlobalItemTemplateCache.ContainsKey(item))
                return false;

            if (template != null)
            {
                found = Inventory.Has(template);

                return count == found;
            }

            return false;
        }

        public bool HasKilled(string value, int number)
        {
            if (MonsterKillCounters.ContainsKey(value)) return MonsterKillCounters[value] >= number;

            return false;
        }

        public Aisling HasManaFor(Spell spell)
        {
            if (CurrentMp >= spell.Template.ManaCost)
                return this;
            Client.SendMessage(0x02, ServerContextBase.Config.NoManaMessage);

            return null;
        }

        public bool HasQuest(string lpName)
        {
            var result = Quests.Any(i => i.Name == lpName);

            return result;
        }

        public bool HasSkill<T>(SkillScope scope) where T : Template, new()
        {
            var obj = new T();

            if (obj is SkillTemplate)
                if ((scope & SkillScope.Assail) == SkillScope.Assail)
                    return SkillBook.Get(i => i != null && i.Template != null
                                                        && i.Template.Type == SkillScope.Assail).Length > 0;

            return false;
        }

        public bool HasVisitedMap(int mapId)
        {
            return DiscoveredMaps.Contains(mapId);
        }

        public bool IsDead()
        {
            var result = Flags.HasFlag(AislingFlags.Ghost);

            return result;
        }

        public bool IsWearing(string item)
        {
            return EquipmentManager.Equipment.Any(i => i.Value != null && i.Value.Item.Template.Name == item);
        }

        public void MakeReactor(string lpName, int lpTimeout)
        {
            ActiveReactors[lpName] = new EphemeralReactor(lpName, lpTimeout);
        }

        public bool ReactedWith(string str)
        {
            return Reactions.ContainsKey(str);
        }

        public void Recover()
        {
            CurrentHp = MaximumHp;
            CurrentMp = MaximumMp;

            Client?.SendStats(StatusFlags.All);
        }

        public void Remove(bool update = false, bool delete = true)
        {
            if (update)
                Show(Scope.NearbyAislingsExludingSelf, new ServerFormat0E(Serial));

            try
            {
                if (delete)
                {
                    var objs = GetObjects(Map,
                        i => i.WithinRangeOf(this) && i.Target != null && i.Target.Serial == Serial,
                        Get.Monsters | Get.Mundanes);
                    if (objs != null)
                        foreach (var obj in objs)
                            obj.Target = null;
                }
            }
            finally
            {
                if (delete)
                    DelObject(this);
            }
        }

        public void ReviveInFront()
        {
            var infront = GetInfront().OfType<Aisling>();

            var action = new ServerFormat1A
            {
                Serial = Serial,
                Number = 0x01,
                Speed = 30
            };

            foreach (var obj in infront)
            {
                if (obj.Serial == Serial)
                    continue;

                if (!obj.LoggedIn)
                    continue;

                obj.RemoveDebuff("skulled", true);
                obj.Client.Revive();
                obj.Animate(5);
            }

            ApplyDamage(this, 0, true, 8);
            Show(Scope.NearbyAislings, action);
        }

        public void SendToHell()
        {
            if (!ServerContextBase.GlobalMapCache.ContainsKey(ServerContextBase.Config.DeathMap))
                return;

            if (CurrentMapId == ServerContextBase.Config.DeathMap)
                return;

            Remains.Owner = this;

            var reepStack = Remains;
            var items = reepStack.Items;

            if (items.Count > 0)
            {
                Remains.ReepItems(items.ToList());
            }
            else
            {
                if (Inventory.Length > 0 || EquipmentManager.Length > 0) Remains.ReepItems();
            }

            for (var i = 0; i < 2; i++)
                RemoveBuffsAndDebuffs();

            WarpToHell();
        }

        public override string ToString()
        {
            return Username;
        }

        public Aisling TrainSpell(Spell lpSpell)
        {
            Client.TrainSpell(lpSpell);
            return this;
        }

        public Aisling UpdateStats(Spell lpSpell)
        {
            Client.SendStats(StatusFlags.All);
            return this;
        }

        public void UpdateStats()
        {
            Client?.SendStats(StatusFlags.All);
        }

        public void WarpToHell()
        {
            Client.LeaveArea(true, true);
            XPos = ServerContextBase.Config.DeathMapX;
            YPos = ServerContextBase.Config.DeathMapY;
            Direction = 0;
            Client.Aisling.CurrentMapId = ServerContextBase.Config.DeathMap;
            Client.EnterArea();

            UpdateStats();
        }

        #region Reactor Stuff
        #endregion
    }
}