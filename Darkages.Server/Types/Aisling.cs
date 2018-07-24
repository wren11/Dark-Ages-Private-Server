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
using Darkages.Network;
using Darkages.Network.ServerFormats;
using Darkages.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Darkages
{
    public class Aisling : Sprite
    {
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
            InvitePrivleges = true;
            LeaderPrivleges = false;
            Remains = new CursedSachel(this);
            ActiveReactor = null;
        }

        public List<ClientGameSettings> GameSettings { get; set; }
        public Bank BankManager { get; set; }
        public int CurrentWeight { get; set; }
        [JsonIgnore] public int MaximumWeight => (int)(_Str * ServerContext.Config.WeightIncreaseModifer);
        public string Username { get; set; }
        public string Password { get; set; }
        public int AbpLevel { get; set; }
        public int AbpTotal { get; set; }
        public int AbpNext { get; set; }
        public int ExpLevel { get; set; }
        public int ExpTotal { get; set; }
        public int ExpNext { get; set; }
        public int Title { get; set; }
        [JsonIgnore] public int AreaID => CurrentMapId;
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
        public Gender Gender { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastLogged { get; set; }
        public AislingFlags Flags { get; set; }
        public GroupStatus PartyStatus { get; set; }
        public List<Quest> Quests { get; set; }
        public ClassStage Stage { get; set; }
        public Class Path { get; set; }
        public Legend LegendBook { get; set; }
        public BodySprite Display { get; set; }
        public SkillBook SkillBook { get; set; }
        public SpellBook SpellBook { get; set; }
        public Inventory Inventory { get; set; }
        public EquipmentManager EquipmentManager { get; set; }
        public ActivityStatus ActiveStatus { get; set; }

        public Dictionary<string, int> MonsterKillCounters 
            = new Dictionary<string, int>();

        public Dictionary<string, DateTime> Reactions
            = new Dictionary<string, DateTime>();

        [JsonIgnore] public bool Skulled => HasDebuff("skulled");

        [JsonIgnore] public Party GroupParty { get; set; }

        [JsonIgnore] public bool IsCastingSpell { get; set; }

        [JsonIgnore] public CastInfo ActiveSpellInfo { get; set; }

        [JsonIgnore] public List<Aisling> PartyMembers => GroupParty?.Members;

        public PortalSession PortalSession { get; set; }

        [JsonIgnore] public int LastMapId { get; set; }

        [JsonIgnore] public bool LeaderPrivleges { get; set; }

        [JsonIgnore] public bool InvitePrivleges { get; set; }

        [JsonIgnore] public int DamageCounter = 0;

        [JsonIgnore] public bool UsingTwoHanded { get; set; }

        [JsonIgnore] [Browsable(false)] public new Position Position => new Position(X, Y);

        [JsonIgnore] public bool Dead => Flags.HasFlag(AislingFlags.Dead);

        [JsonIgnore] public bool Invisible => Flags.HasFlag(AislingFlags.Invisible);

        [JsonIgnore] public ConcurrentDictionary<int, Sprite> ViewFrustrum = new ConcurrentDictionary<int, Sprite>();

        [JsonIgnore] public CursedSachel Remains { get; set; }

        [JsonIgnore] public ExchangeSession Exchange { get; set; }

        [JsonIgnore]
        public Reactor ActiveReactor { get; set; }

        [JsonIgnore]
        public Trap[] Traps => Trap.TrapCache.Select(i => i.Value)
            .ToList().Where(i => i.Creator.Serial == Serial).ToArray();

        [JsonIgnore]
        public int SelectedTrapIndex = 0;

        [JsonIgnore]
        public Trap SelectedTrap => Traps[SelectedTrapIndex++ % Traps.Length];

        public void CycleTraps() => SelectedTrapIndex++;


        [JsonIgnore]
        public List<Sprite> ViewableObjects
        {
            get
            {
                return ViewFrustrum.Select(i => i.Value).ToList();
            }
        }

        [JsonIgnore]
        public bool ReactorActive { get; set; }

        public void Recover()
        {
            CurrentHp = MaximumHp;
            CurrentMp = MaximumMp;

            Client?.SendStats(StatusFlags.All);
        }

        public bool InsideView(Sprite obj)
        {
            if (ViewFrustrum.Count == 0)
                return false;

            return ViewFrustrum.ContainsKey(obj.Serial);
        }

        public bool RemoveFromView(Sprite obj)
        {
            return ViewFrustrum.TryRemove(obj.Serial, out var removed);
        }

        public void GoHome()
        {
            var DestinationMap = ServerContext.Config.TransitionZone;

            if (ServerContext.GlobalMapCache.ContainsKey(DestinationMap))
            {
                var targetMap = ServerContext.GlobalMapCache[DestinationMap];

                Client.LeaveArea(true, true);
                Client.Aisling.X = ServerContext.Config.TransitionPointX;
                Client.Aisling.Y = ServerContext.Config.TransitionPointY;
                Client.Aisling.CurrentMapId = DestinationMap;
                Client.EnterArea();
                Client.Refresh();
            }
            Client.CloseDialog();
        }

        public bool View(Sprite obj)
        {
            if (!InsideView(obj))
            {
                ViewFrustrum.TryAdd(obj.Serial, obj);

                    if (obj is Monster)
                        (obj as Monster).Script
                            ?.OnApproach(Client);

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
                    spell.Script.Arguments = info.Data;

                var target = GetObject(i => i.Serial == info.Target, Get.Monsters | Get.Aislings | Get.Mundanes);
                spell.InUse = true;


                if (spell.Script != null)
                {
                    if (target != null)
                    {
                        if (target is Aisling)
                            spell.Script.OnUse(this, target as Aisling);
                        if (target is Monster)
                            spell.Script.OnUse(this, target as Monster);
                        if (target is Mundane)
                            spell.Script.OnUse(this, target as Mundane);
                    }
                    else
                    {
                        spell.Script.OnUse(this, this);
                    }
                }
            }

            spell.NextAvailableUse = DateTime.UtcNow.AddSeconds(info.SpellLines > 0 ? 1 : 0.3);
            spell.InUse = false;
        }

        public bool hasKilled(string value, int number)
        {
            if (MonsterKillCounters.ContainsKey(value))
            {
                return MonsterKillCounters[value] >= number;
            }

            return false;
        }

        public static Aisling Create()
        {
            var fractions = Enum.GetValues(typeof(Fraction));
            var randomFraction = (int)fractions.GetValue(Generator.Random.Next(fractions.Length));

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
                CurrentMapId = ServerContext.Config.StartingMap,
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
                X = ServerContext.Config.StartingPosition.X,
                Y = ServerContext.Config.StartingPosition.Y,
                Nation = (byte)randomFraction,
            };

            int idx = 1;
            foreach (var skill in ServerContext.GlobalSkillTemplateCache.Keys)
            {
                if (ServerContext.GlobalSkillTemplateCache[skill].Pane == Pane.Tools)
                    continue;

                Skill.GiveTo(result, skill, 100);
            }
            foreach (var skill in ServerContext.GlobalSkillTemplateCache.Keys)
            {
                if (ServerContext.GlobalSkillTemplateCache[skill].Pane == Pane.Tools)
                {
                    Skill.GiveTo(result, skill, (byte)(72 + idx));
                    idx++;
                }
            }
            foreach (var spell in ServerContext.GlobalSpellTemplateCache.Keys)
            {
                if (ServerContext.GlobalSpellTemplateCache[spell].Pane == Pane.Tools)
                    continue;

                Spell.GiveTo(result, spell, 100);
            }
            idx = 1;
            foreach (var spell in ServerContext.GlobalSpellTemplateCache.Keys)
            {
                if (ServerContext.GlobalSpellTemplateCache[spell].Pane == Pane.Tools)
                {
                    Spell.GiveTo(result, spell, (byte)(72 + idx));
                    idx++;
                }
            }

            Spell.GiveTo(result, "Needle Trap", 100);

            if (DateTime.UtcNow.Year <= 2019)
            {
                result.LegendBook.AddLegend(new Legend.LegendItem
                {
                    Category = "Event",
                    Color = (byte)LegendColor.DarkPurple,
                    Icon = (byte)LegendIcon.Victory,
                    Value = string.Format("Aisling Age of Aquarius")
                });
            }

            if (result.Nation == 1)
            {
                result.LegendBook.AddLegend(new Legend.LegendItem
                {
                    Category = "Event",
                    Color = (byte)LegendColor.Orange,
                    Icon = (byte)LegendIcon.Community,
                    Value = string.Format("Lorule Citizen")
                });
            }
            else if (result.Nation == 2)
            {
                result.LegendBook.AddLegend(new Legend.LegendItem
                {
                    Category = "Event",
                    Color = (byte)LegendColor.LightGreen,
                    Icon = (byte)LegendIcon.Community,
                    Value = string.Format("Lividia Citizen")
                });
            }
            else if (result.Nation == 3)
            {
                result.LegendBook.AddLegend(new Legend.LegendItem
                {
                    Category = "Event",
                    Color = (byte)LegendColor.Darkgreen,
                    Icon = (byte)LegendIcon.Community,
                    Value = string.Format("Amongst the Exile.")
                });
            }

            return result;
        }

        /// <summary>
        ///     Removes Aisling, Sends Remove packet to nearby aislings
        ///     and removes itself from the ObjectManager.
        /// </summary>
        public void Remove(bool update = false, bool delete = true)
        {
            if (Map != null)
            {
                Map.Update(X, Y, TileContent.None);
                ServerContext.GlobalMapCache[Map.ID].Update(X, Y, TileContent.None);
            }

            if (update)
                Show(Scope.NearbyAislingsExludingSelf, new ServerFormat0E(Serial));

            try
            {
                var objs = GetObjects(i => i.WithinRangeOf(this) && i.Target != null && i.Target.Serial == Serial, Get.Monsters | Get.Mundanes);

                foreach (var obj in objs)
                    obj.Target = null;
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


        /// <summary>
        ///     This Method will return all skills that are assail-like, Assail, Clobber, Ect.
        /// </summary>
        public Skill[] GetAssails(SkillScope scope)
        {
            if ((scope & SkillScope.Assail) == SkillScope.Assail)
                return SkillBook.Get(i => i != null && i.Template != null
                                                    && i.Template.Type == SkillScope.Assail).ToArray();

            return null;
        }

        public void UpdateStats()
        {
            Client?.SendStats(StatusFlags.All);
        }

        /// <summary>
        /// This entire exchange routine was shamelessly copy pasted from Kojasou's Server Project.
        /// (Yes I'm way to lazy to write this myself when it's already been done correctly.)
        /// Credits: https://github.com/kojasou/wewladh
        /// </summary>
        public void CancelExchange()
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
                if (item.GiveTo(trader, true)) { }

            foreach (var item in itemsA)
                if (item.GiveTo(this, true)) { }


            GoldPoints += goldA;
            trader.GoldPoints += goldB;



            if (trader.GoldPoints > ServerContext.Config.MaxCarryGold)
                trader.GoldPoints = ServerContext.Config.MaxCarryGold;
            if (GoldPoints > ServerContext.Config.MaxCarryGold)
                GoldPoints = ServerContext.Config.MaxCarryGold;

            trader.Client.SendStats(StatusFlags.StructC);
            Client.SendStats(StatusFlags.StructC);


            var packet = new NetworkPacketWriter();
            packet.Write((byte)0x42);
            packet.Write((byte)0x00);

            packet.Write((byte)0x04);
            packet.Write((byte)0x00);
            packet.WriteStringA("Trade was aborted.");
            Client.Send(packet);

            packet = new NetworkPacketWriter();
            packet.Write((byte)0x42);
            packet.Write((byte)0x00);

            packet.Write((byte)0x04);
            packet.Write((byte)0x01);
            packet.WriteStringA("Trade was aborted.");
            trader.Client.Send(packet);
        }

        /// <summary>
        /// This entire exchange routine was shamelessly copy pasted from Kojasou's Server Project.
        /// (Yes I'm way to lazy to write this myself when it's already been done correctly.)
        /// Credits: https://github.com/kojasou/wewladh
        /// </summary>
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
            {
                if (item.GiveTo(this, true))
                {

                }
            }

            foreach (var item in itemsA)
            {
                if (item.GiveTo(trader, true))
                {

                }
            }


            GoldPoints += goldB;
            trader.GoldPoints += goldA;

            if (trader.GoldPoints > ServerContext.Config.MaxCarryGold)
                trader.GoldPoints = ServerContext.Config.MaxCarryGold;
            if (GoldPoints > ServerContext.Config.MaxCarryGold)
                GoldPoints = ServerContext.Config.MaxCarryGold;

            exchangeA.Items.Clear();
            exchangeB.Items.Clear();

            trader.Client?.SendStats(StatusFlags.All);
            Client?.SendStats(StatusFlags.All);

        }

        public bool ReactedWith(string str) => Reactions.ContainsKey(str);
    }
}
