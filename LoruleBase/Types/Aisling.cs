#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Darkages.Common;
using Darkages.Network;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

        private readonly object syncLock = new object();

        [JsonIgnore] public HashSet<Sprite> View = new HashSet<Sprite>();


        public Aisling()
        {
            OffenseElement = ElementManager.Element.None;
            DefenseElement = ElementManager.Element.None;
            Clan = string.Empty;
            Flags = AislingFlags.Normal;
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

        public bool GameMaster { get; set; }

        public List<ClientGameSettings> GameSettings { get; set; }

        public Bank BankManager { get; set; }

        public int CurrentWeight { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public int AbpLevel { get; set; }

        public int AbpTotal { get; set; }

        public int AbpNext { get; set; }

        public int ExpLevel { get; set; }

        public uint ExpTotal { get; set; }

        public uint ExpNext { get; set; }

        public int Title { get; set; }

        public int ClassID { get; set; }

        public int GamePoints { get; set; }

        public int GoldPoints { get; set; }

        public int StatPoints { get; set; }

        public int BodyColor { get; set; }

        public int BodyStyle { get; set; }

        public int FaceColor { get; set; }

        public int FaceStyle { get; set; }

        public byte HairColor { get; set; }

        public byte HairStyle { get; set; }

        public byte Boots { get; set; }

        public int Helmet { get; set; }

        public byte Shield { get; set; }

        public ushort Weapon { get; set; }

        public ushort Armor { get; set; }

        public byte OverCoat { get; set; }

        public byte Pants { get; set; }

        public byte[] PictureData { get; set; }

        public string ProfileMessage { get; set; }

        public bool LoggedIn { get; set; }

        public byte Nation { get; set; }

        public string Clan { get; set; }

        public byte Resting { get; set; }

        public bool TutorialCompleted { get; set; }

        public byte Blind { get; set; }

        public byte HeadAccessory1 { get; set; }

        public byte HeadAccessory2 { get; set; }

        public string ClanTitle { get; set; }

        public string ClanRank { get; set; }

        public byte BootColor { get; set; }

        public byte NameColor { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Gender Gender { get; set; }

        public DateTime Created { get; set; }

        public DateTime LastLogged { get; set; }

        public int World { get; set; } = 1;

        [JsonConverter(typeof(StringEnumConverter))]
        public AislingFlags Flags { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public GroupStatus PartyStatus { get; set; }

        public List<Quest> Quests { get; set; }

        public ClassStage Stage { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Class Path { get; set; }

        public Legend LegendBook { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public BodySprite Display { get; set; }

        public SkillBook SkillBook { get; set; }

        public SpellBook SpellBook { get; set; }

        public Inventory Inventory { get; set; }

        public EquipmentManager EquipmentManager { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ActivityStatus ActiveStatus { get; set; }

        public List<int> DiscoveredMaps { get; set; }

        public PortalSession PortalSession { get; set; }

        [JsonIgnore]
        public int MaximumWeight => (int) (ExpLevel / 4 + _Str + ServerContextBase.Config.WeightIncreaseModifer);

        [JsonIgnore] public bool ProfileOpen { get; set; }

        [JsonIgnore] public int AreaID => CurrentMapId;

        [JsonIgnore] public bool Skulled => HasDebuff("skulled");


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

        [JsonIgnore] public bool IsCastingSpell { get; set; }

        [JsonIgnore] public CastInfo ActiveSpellInfo { get; set; }

        [JsonIgnore] public List<Aisling> PartyMembers => GroupParty?.PartyMembers;

        [JsonIgnore] public int FieldNumber { get; set; } = 1;

        [JsonIgnore] public int LastMapId { get; set; }

        [JsonIgnore]
        public bool LeaderPrivileges
        {
            get
            {
                if (!ServerContextBase.GlobalGroupCache.ContainsKey(GroupId))
                    return false;

                var group = ServerContextBase.GlobalGroupCache[GroupId];
                return group != null &&
                       string.Equals(group.LeaderName, Username, StringComparison.CurrentCultureIgnoreCase);
            }
        }


        [JsonIgnore] public bool UsingTwoHanded { get; set; }

        [JsonIgnore] [Browsable(false)] public new Position Position => new Position(XPos, YPos);

        [JsonIgnore] public bool Dead => Flags.HasFlag(AislingFlags.Dead);

        [JsonIgnore] public bool Invisible => Flags.HasFlag(AislingFlags.Invisible);


        public CursedSachel Remains { get; set; }

        [JsonIgnore] public ExchangeSession Exchange { get; set; }


        [JsonIgnore] public Reactor ActiveReactor { get; set; }

        public void Assail()
        {
            if (Client != null) GameServer.ActivateAssails(Client);
        }

        public void MakeReactor(string lpName, int lpTimeout)
        {
            ActiveReactors[lpName] = new EphemeralReactor(lpName, lpTimeout);
        }

        public bool HasQuest(string lpName)
        {
            var result = Quests.Any(i => i.Name == lpName);

            return result;
        }

        public bool HasCompletedQuest(string lpName)
        {
            return Quests.Any(i => i.Name == lpName && i.Completed);
        }

        public void CompleteQuest(string lpName)
        {
            var obj = Quests.Find(i => i.Name == lpName);
            obj.Completed = true;
        }

        public Quest GetQuest(string name)
        {
            return Quests.FirstOrDefault(i => i.Name == name);
        }


        public bool AcceptQuest(Quest lpQuest)
        {
            lock (syncLock)
            {
                if (!Quests.Any(i => i.Name == lpQuest.Name))
                {
                    Quests.Add(lpQuest);

                    return true;
                }
            }

            return false;
        }


        public bool IsDead()
        {
            var result = Flags.HasFlag(AislingFlags.Dead);

            return result;
        }

        public bool CanSeeGhosts()
        {
            return IsDead();
        }

        public bool CanSeeHidden()
        {
            return Flags.HasFlag(AislingFlags.SeeInvisible);
        }

        public bool HasVisitedMap(int mapId)
        {
            return DiscoveredMaps.Contains(mapId);
        }

        public void Recover()
        {
            CurrentHp = MaximumHp;
            CurrentMp = MaximumMp;

            Client?.SendStats(StatusFlags.All);
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

        public bool IsWearing(string item)
        {
            return EquipmentManager.Equipment.Any(i => i.Value != null && i.Value.Item.Template.Name == item);
        }

        public void GoHome()
        {
            var DestinationMap = ServerContextBase.Config.TransitionZone;

            if (ServerContextBase.GlobalMapCache.ContainsKey(DestinationMap))
            {
                var targetMap = ServerContextBase.GlobalMapCache[DestinationMap];

                Client.LeaveArea(true, true);
                Client.Aisling.XPos = ServerContextBase.Config.TransitionPointX;
                Client.Aisling.YPos = ServerContextBase.Config.TransitionPointY;
                Client.Aisling.CurrentMapId = DestinationMap;
                Client.EnterArea();
                Client.Refresh();
            }

            Client.CloseDialog();
        }

        public Aisling HasManaFor(Spell spell)
        {
            if (CurrentMp >= spell.Template.ManaCost)
                return this;
            Client.SendMessage(0x02, ServerContextBase.Config.NoManaMessage);

            return null;
        }

        public Aisling UpdateStats(Spell lpSpell)
        {
            Client.SendStats(StatusFlags.All);
            return this;
        }

        public Aisling TrainSpell(Spell lpSpell)
        {
            Client.TrainSpell(lpSpell);
            return this;
        }

        public Aisling GiveHealth(Sprite target, int value)
        {
            target.CurrentHp += value;

            if (target.CurrentHp > target.MaximumHp) target.CurrentHp = target.MaximumHp;

            return this;
        }

        public Aisling Cast(Spell spell, Sprite target, byte actionSpeed = 30)
        {
            var action = new ServerFormat1A
            {
                Serial = Serial,
                Number = (byte) (Path == Class.Priest ? 0x80 : Path == Class.Wizard ? 0x88 : 0x06),
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
                        Health = (ushort) (100 * target.CurrentHp / target.MaximumHp),
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
                        if (target is Aisling tobj)
                            foreach (var script in spell.Scripts.Values)
                                script.OnUse(this, target as Aisling);

                        if (target is Monster aobj)
                            foreach (var script in spell.Scripts.Values)
                                script.OnUse(this, aobj);

                        if (target is Mundane)
                            foreach (var script in spell.Scripts.Values)
                                script.OnUse(this, target as Mundane);
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

        public void DestroyReactor(Reactor Actor)
        {
            if (Reactions.ContainsKey(Actor.Name))
                Reactions.Remove(Actor.Name);

            ActiveReactor = null;
            ReactorActive = false;
        }

        public bool HasKilled(string value, int number)
        {
            if (MonsterKillCounters.ContainsKey(value)) return MonsterKillCounters[value] >= number;

            return false;
        }

        public static Aisling Create()
        {
            var fractions = Enum.GetValues(typeof(Fraction));
            var randomFraction = (int) fractions.GetValue(Generator.Random.Next(fractions.Length));

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
                Flags = 0,
                CurrentMapId = ServerContextBase.Config.StartingMap,
                ClassID = 0,
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
                EquipmentManager = new EquipmentManager(null),
                BankManager = new Bank(),
                Created = DateTime.UtcNow,
                LastLogged = DateTime.UtcNow,
                XPos = ServerContextBase.Config.StartingPosition.X,
                YPos = ServerContextBase.Config.StartingPosition.Y,
                Nation = (byte) randomFraction,
                AnimalForm = AnimalForm.None
            };

            if (ServerContextBase.Config.GiveAssailOnCreate) Skill.GiveTo(result, "Assail", 1);

            if (ServerContextBase.Config.DevMode)
            {
                foreach (var temp in ServerContextBase.GlobalSpellTemplateCache) Spell.GiveTo(result, temp.Value.Name);
                foreach (var temp in ServerContextBase.GlobalSkillTemplateCache) Skill.GiveTo(result, temp.Value.Name);
            }

            if (result.Nation == 1)
                result.LegendBook.AddLegend(new Legend.LegendItem
                {
                    Category = "Event",
                    Color = (byte) LegendColor.Orange,
                    Icon = (byte) LegendIcon.Community,
                    Value = "Lorule Citizen."
                });
            else if (result.Nation == 2)
                result.LegendBook.AddLegend(new Legend.LegendItem
                {
                    Category = "Event",
                    Color = (byte) LegendColor.LightGreen,
                    Icon = (byte) LegendIcon.Community,
                    Value = "Lividia Citizen."
                });
            else if (result.Nation == 3)
                result.LegendBook.AddLegend(new Legend.LegendItem
                {
                    Category = "Event",
                    Color = (byte) LegendColor.Darkgreen,
                    Icon = (byte) LegendIcon.Community,
                    Value = "Amongst the Exile."
                });

            return result;
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

        public bool HasSkill<T>(SkillScope scope) where T : Template, new()
        {
            var obj = new T();

            if (obj is SkillTemplate)
                if ((scope & SkillScope.Assail) == SkillScope.Assail)
                    return SkillBook.Get(i => i != null && i.Template != null
                                                        && i.Template.Type == SkillScope.Assail).Length > 0;

            return false;
        }


        public Skill[] GetAssails()
        {
            return SkillBook.Get(i => i != null && i.Template != null
                                                && i.Template.Type == SkillScope.Assail).ToArray();
        }

        public void UpdateStats()
        {
            Client?.SendStats(StatusFlags.All);
        }


        public bool CastDeath()
        {
            if (!Client.Aisling.Flags.HasFlag(AislingFlags.Dead))
            {
                LastMapId = CurrentMapId;
                LastPosition = Position;

                Client.CloseDialog();
                Client.AislingToGhostForm();


                return true;
            }

            return false;
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

            Client.LeaveArea(true, true);
            XPos = ServerContextBase.Config.DeathMapX;
            YPos = ServerContextBase.Config.DeathMapY;
            Direction = 0;
            Client.Aisling.CurrentMapId = ServerContextBase.Config.DeathMap;
            Client.EnterArea();

            UpdateStats();
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


            var packet = new NetworkPacketWriter();
            packet.Write((byte) 0x42);
            packet.Write((byte) 0x00);

            packet.Write((byte) 0x04);
            packet.Write((byte) 0x00);
            packet.WriteStringA("Trade was aborted.");
            Client.Send(packet);

            packet = new NetworkPacketWriter();
            packet.Write((byte) 0x42);
            packet.Write((byte) 0x00);

            packet.Write((byte) 0x04);
            packet.Write((byte) 0x01);
            packet.WriteStringA("Trade was aborted.");
            trader.Client.Send(packet);
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

        public bool ReactedWith(string str)
        {
            return Reactions.ContainsKey(str);
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

        public override string ToString()
        {
            return Username;
        }

        #region Reactor Stuff

        [JsonIgnore] public bool ReactorActive { get; set; }

        [JsonIgnore] public bool CanReact { get; set; }

        [JsonIgnore] public DialogSequence ActiveSequence { get; set; }

        public AnimalForm AnimalForm { get; set; }

        #endregion
    }
}