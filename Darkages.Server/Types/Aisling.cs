// ************************************************************************
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
// *************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Darkages.Common;
using Darkages.Network;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Types;
using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Darkages
{
    /// <summary>
    ///     The Main Aisling Class, Contains all Coupled Fields and Members for Interacting with the Aisling Sprite Object.
    /// </summary>
    /// <seealso cref="Darkages.Types.Sprite" />
    public class Aisling : Sprite
    {
        /// <summary>
        ///     The active reactors
        /// </summary>
        public Dictionary<string, EphemeralReactor> ActiveReactors = new Dictionary<string, EphemeralReactor>();

        /// <summary>
        ///     The damage counter
        /// </summary>
        [JsonIgnore] public int DamageCounter = 0;

        /// <summary>
        ///     The monster kill counters
        /// </summary>
        public Dictionary<string, int> MonsterKillCounters = new Dictionary<string, int>();

        /// <summary>
        ///     The popups
        /// </summary>
        [JsonIgnore] public List<Popup> Popups = new List<Popup>();

        /// <summary>
        ///     The reactions
        /// </summary>
        public Dictionary<string, DateTime> Reactions = new Dictionary<string, DateTime>();

        /// <summary>
        ///     The view
        /// </summary>
        [JsonIgnore] public HashSet<Sprite> View = new HashSet<Sprite>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="Aisling" /> class.
        /// </summary>
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
            DiscoveredMaps = new List<int>();
            Popups = new List<Popup>();
        }

        /// <summary>
        ///     Gets or sets a value indicating whether The Aisling is a [game master].
        /// </summary>
        /// <value><c>true</c> if [game master]; otherwise, <c>false</c>.</value>
        public bool GameMaster { get; set; }

        /// <summary>
        ///     Gets or sets the game settings.
        /// </summary>
        /// <value>The game settings.</value>
        [BsonIgnore] public List<ClientGameSettings> GameSettings { get; set; }

        /// <summary>
        ///     Gets or sets the <strong>bank</strong> manager.
        /// </summary>
        /// <value>The bank manager.</value>
        public Bank BankManager { get; set; }

        /// <summary>
        ///     Gets or sets the current weight.
        /// </summary>
        /// <value>The current weight.</value>
        public int CurrentWeight { get; set; }

        /// <summary>
        ///     Gets or sets the username.
        /// </summary>
        /// <value>The username.</value>
        public string Username { get; set; }

        /// <summary>
        ///     Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        public string Password { get; set; }

        /// <summary>
        ///     Gets or sets the abp level.
        /// </summary>
        /// <value>The abp level.</value>
        public int AbpLevel { get; set; }

        /// <summary>
        ///     Gets or sets the abp total.
        /// </summary>
        /// <value>The abp total.</value>
        public int AbpTotal { get; set; }

        /// <summary>
        ///     Gets or sets the abp next.
        /// </summary>
        /// <value>The abp next.</value>
        public int AbpNext { get; set; }

        /// <summary>
        ///     Gets or sets the exp level.
        /// </summary>
        /// <value>The exp level.</value>
        public int ExpLevel { get; set; }

        /// <summary>
        ///     Gets or sets the exp total.
        /// </summary>
        /// <value>The exp total.</value>
        public uint ExpTotal { get; set; }

        /// <summary>
        ///     Gets or sets the exp next.
        /// </summary>
        /// <value>The exp next.</value>
        public uint ExpNext { get; set; }

        /// <summary>
        ///     Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public int Title { get; set; }

        /// <summary>
        ///     Gets or sets the class identifier.
        /// </summary>
        /// <value>The class identifier.</value>
        public int ClassID { get; set; }

        /// <summary>
        ///     Gets or sets the game points.
        /// </summary>
        /// <value>The game points.</value>
        public int GamePoints { get; set; }

        /// <summary>
        ///     Gets or sets the gold points.
        /// </summary>
        /// <value>The gold points.</value>
        public int GoldPoints { get; set; }

        /// <summary>
        ///     Gets or sets the stat points.
        /// </summary>
        /// <value>The stat points.</value>
        public int StatPoints { get; set; }

        /// <summary>
        ///     Gets or sets the color of the body.
        /// </summary>
        /// <value>The color of the body.</value>
        public int BodyColor { get; set; }

        /// <summary>
        ///     Gets or sets the body style.
        /// </summary>
        /// <value>The body style.</value>
        public int BodyStyle { get; set; }

        /// <summary>
        ///     Gets or sets the color of the face.
        /// </summary>
        /// <value>The color of the face.</value>
        public int FaceColor { get; set; }

        /// <summary>
        ///     Gets or sets the face style.
        /// </summary>
        /// <value>The face style.</value>
        public int FaceStyle { get; set; }

        /// <summary>
        ///     Gets or sets the color of the hair.
        /// </summary>
        /// <value>The color of the hair.</value>
        public byte HairColor { get; set; }

        /// <summary>
        ///     Gets or sets the hair style.
        /// </summary>
        /// <value>The hair style.</value>
        public byte HairStyle { get; set; }

        /// <summary>
        ///     Gets or sets the boots.
        /// </summary>
        /// <value>The boots.</value>
        public byte Boots { get; set; }

        /// <summary>
        ///     Gets or sets the helmet.
        /// </summary>
        /// <value>The helmet.</value>
        public int Helmet { get; set; }

        /// <summary>
        ///     Gets or sets the shield.
        /// </summary>
        /// <value>The shield.</value>
        public byte Shield { get; set; }

        /// <summary>
        ///     Gets or sets the weapon.
        /// </summary>
        /// <value>The weapon.</value>
        public ushort Weapon { get; set; }

        /// <summary>
        ///     Gets or sets the armor.
        /// </summary>
        /// <value>The armor.</value>
        public ushort Armor { get; set; }

        /// <summary>
        ///     Gets or sets the over coat.
        /// </summary>
        /// <value>The over coat.</value>
        public byte OverCoat { get; set; }

        /// <summary>
        ///     Gets or sets the pants.
        /// </summary>
        /// <value>The pants.</value>
        public byte Pants { get; set; }

        /// <summary>
        ///     Gets or sets the picture data.
        /// </summary>
        /// <value>The picture data.</value>
        public byte[] PictureData { get; set; }

        /// <summary>
        ///     Gets or sets the profile message.
        /// </summary>
        /// <value>The profile message.</value>
        public string ProfileMessage { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [logged in].
        /// </summary>
        /// <value><c>true</c> if [logged in]; otherwise, <c>false</c>.</value>
        public bool LoggedIn { get; set; }

        /// <summary>
        ///     Gets or sets the nation.
        /// </summary>
        /// <value>The nation.</value>
        public byte Nation { get; set; }

        /// <summary>
        ///     Gets or sets the clan.
        /// </summary>
        /// <value>The clan.</value>
        public string Clan { get; set; }

        /// <summary>
        ///     Gets or sets the resting.
        /// </summary>
        /// <value>The resting.</value>
        public byte Resting { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [tutorial completed].
        /// </summary>
        /// <value><c>true</c> if [tutorial completed]; otherwise, <c>false</c>.</value>
        public bool TutorialCompleted { get; set; }

        /// <summary>
        ///     Gets or sets the blind.
        /// </summary>
        /// <value>The blind.</value>
        public byte Blind { get; set; }

        /// <summary>
        ///     Gets or sets the head accessory1.
        /// </summary>
        /// <value>The head accessory1.</value>
        public byte HeadAccessory1 { get; set; }

        /// <summary>
        ///     Gets or sets the head accessory2.
        /// </summary>
        /// <value>The head accessory2.</value>
        public byte HeadAccessory2 { get; set; }

        /// <summary>
        ///     Gets or sets the clan title.
        /// </summary>
        /// <value>The clan title.</value>
        public string ClanTitle { get; set; }

        /// <summary>
        ///     Gets or sets the clan rank.
        /// </summary>
        /// <value>The clan rank.</value>
        public string ClanRank { get; set; }

        /// <summary>
        ///     Gets or sets the color of the boot.
        /// </summary>
        /// <value>The color of the boot.</value>
        public byte BootColor { get; set; }

        /// <summary>
        ///     Gets or sets the color of the name.
        /// </summary>
        /// <value>The color of the name.</value>
        public byte NameColor { get; set; }

        /// <summary>
        ///     Gets or sets the gender.
        /// </summary>
        /// <value>The gender.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public Gender Gender { get; set; }

        /// <summary>
        ///     Gets or sets the created.
        /// </summary>
        /// <value>The created.</value>
        public DateTime Created { get; set; }

        /// <summary>
        ///     Gets or sets the last logged.
        /// </summary>
        /// <value>The last logged.</value>
        public DateTime LastLogged { get; set; }

        /// <summary>
        /// What World we are currently on.
        /// </summary>
        public int World { get; set; } = 1;

        /// <summary>
        ///     Gets or sets the flags.
        /// </summary>
        /// <value>The flags.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public AislingFlags Flags { get; set; }

        /// <summary>
        ///     Gets or sets the party status.
        /// </summary>
        /// <value>The party status.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public GroupStatus PartyStatus { get; set; }

        /// <summary>
        ///     Gets or sets the quests.
        /// </summary>
        /// <value>The quests.</value>
        public List<Quest> Quests { get; set; }

        /// <summary>
        ///     Gets or sets the stage.
        /// </summary>
        /// <value>The stage.</value>
        public ClassStage Stage { get; set; }

        /// <summary>
        ///     Gets or sets the path.
        /// </summary>
        /// <value>The path.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public Class Path { get; set; }

        /// <summary>
        ///     Gets or sets the legend book.
        /// </summary>
        /// <value>The legend book.</value>
        public Legend LegendBook { get; set; }

        /// <summary>
        ///     Gets or sets the display.
        /// </summary>
        /// <value>The display.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public BodySprite Display { get; set; }

        /// <summary>
        ///     Gets or sets the skill book.
        /// </summary>
        /// <value>The skill book.</value>
        public SkillBook SkillBook { get; set; }

        /// <summary>
        ///     Gets or sets the spell book.
        /// </summary>
        /// <value>The spell book.</value>
        public SpellBook SpellBook { get; set; }

        /// <summary>
        ///     Gets or sets the inventory.
        /// </summary>
        /// <value>The inventory.</value>
        public Inventory Inventory { get; set; }

        /// <summary>
        ///     Gets or sets the equipment manager.
        /// </summary>
        /// <value>The equipment manager.</value>
        public EquipmentManager EquipmentManager { get; set; }

        /// <summary>
        ///     Gets or sets the active status.
        /// </summary>
        /// <value>The active status.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public ActivityStatus ActiveStatus { get; set; }

        /// <summary>
        ///     Gets or sets the discovered maps.
        /// </summary>
        /// <value>The discovered maps.</value>
        public List<int> DiscoveredMaps { get; set; }

        /// <summary>
        ///     Gets or sets the portal session.
        /// </summary>
        /// <value>The portal session.</value>
        public PortalSession PortalSession { get; set; }

        /// <summary>
        ///     Gets the maximum weight.
        /// </summary>
        /// <value>The maximum weight.</value>
        [JsonIgnore]
        public int MaximumWeight => (int) (_Str * ServerContext.Config.WeightIncreaseModifer);

        /// <summary>
        ///     Gets or sets a value indicating whether [profile open].
        /// </summary>
        /// <value><c>true</c> if [profile open]; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool ProfileOpen { get; set; }

        /// <summary>
        ///     Gets the area identifier.
        /// </summary>
        /// <value>The area identifier.</value>
        [JsonIgnore]
        public int AreaID => CurrentMapId;

        /// <summary>
        ///     Gets a value indicating whether this <see cref="Aisling" /> is skulled.
        /// </summary>
        /// <value><c>true</c> if skulled; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool Skulled => HasDebuff("skulled");

        /// <summary>
        ///     Gets or sets the group party.
        /// </summary>
        /// <value>The group party.</value>
        [JsonIgnore]
        public Party GroupParty { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is casting spell.
        /// </summary>
        /// <value><c>true</c> if this instance is casting spell; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool IsCastingSpell { get; set; }

        /// <summary>
        ///     Gets or sets the active spell information.
        /// </summary>
        /// <value>The active spell information.</value>
        [JsonIgnore]
        public CastInfo ActiveSpellInfo { get; set; }

        /// <summary>
        ///     Gets the party members.
        /// </summary>
        /// <value>The party members.</value>
        [JsonIgnore]
        public List<Aisling> PartyMembers => GroupParty?.Members;

        /// <summary>
        ///     Gets or sets the field number.
        /// </summary>
        /// <value>The field number.</value>
        [JsonIgnore]
        public int FieldNumber { get; set; } = 1;

        /// <summary>
        ///     Gets or sets the last map identifier.
        /// </summary>
        /// <value>The last map identifier.</value>
        [JsonIgnore]
        public int LastMapId { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [leader privleges].
        /// </summary>
        /// <value><c>true</c> if [leader privleges]; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool LeaderPrivleges { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [invite privleges].
        /// </summary>
        /// <value><c>true</c> if [invite privleges]; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool InvitePrivleges { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [using two handed].
        /// </summary>
        /// <value><c>true</c> if [using two handed]; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool UsingTwoHanded { get; set; }

        /// <summary>
        ///     Gets the position.
        /// </summary>
        /// <value>The position.</value>
        [JsonIgnore]
        [Browsable(false)]
        public new Position Position => new Position(XPos, YPos);

        /// <summary>
        ///     Gets a value indicating whether this <see cref="Aisling" /> is dead.
        /// </summary>
        /// <value><c>true</c> if dead; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool Dead => Flags.HasFlag(AislingFlags.Dead);

        /// <summary>
        ///     Gets a value indicating whether this <see cref="Aisling" /> is invisible.
        /// </summary>
        /// <value><c>true</c> if invisible; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool Invisible => Flags.HasFlag(AislingFlags.Invisible);

        /// <summary>
        ///     Gets or sets the remains.
        /// </summary>
        /// <value>The remains.</value>
        [JsonIgnore]
        public CursedSachel Remains { get; set; }

        /// <summary>
        ///     Gets or sets the exchange.
        /// </summary>
        /// <value>The exchange.</value>
        [JsonIgnore]
        public ExchangeSession Exchange { get; set; }


        /// <summary>
        ///     Gets or sets the active reactor.
        /// </summary>
        /// <value>The active reactor.</value>
        [JsonIgnore]
        public Reactor ActiveReactor { get; set; }

        /// <summary>
        ///     Gets my traps.
        /// </summary>
        /// <value>My traps.</value>
        [JsonIgnore]
        public List<Trap> MyTraps => Trap.Traps.Select(i => i.Value).Where(i => i.Owner.Serial == Serial).ToList();

        /// <summary>
        ///     Assail, (Space-Bar) Activating all Assails.
        /// </summary>
        public void Assail()
        {
            if (Client != null) GameServer.ActivateAssails(Client);
        }

        /// <summary>
        ///     Makes the reactor.
        /// </summary>
        /// <param name="lpName">The Yaml Script Name</param>
        /// <param name="lpTimeout">Number of seconds until the Reactor expires.</param>
        /// <returns>void</returns>
        public void MakeReactor(string lpName, int lpTimeout)
        {
            ActiveReactors[lpName] = new EphemeralReactor(lpName, lpTimeout);
        }

        /// <summary>
        ///     Determines whether the specified lp name has quest.
        /// </summary>
        /// <param name="lpName">The Quest key to check for.</param>
        /// <returns><c>true</c> if the specified Quest Exists; otherwise, <c>false</c>.</returns>
        public bool HasQuest(string lpName)
        {
            var result = Quests.Any(i => i.Name == lpName);

            return result;
        }

        /// <summary>
        ///     Determines whether [has completed quest] [the specified lp name].
        /// </summary>
        /// <param name="lpName">The Quest key to check for Completion.</param>
        /// <returns><c>true</c> if [has completed quest] [lpName]; otherwise, <c>false</c>.</returns>
        /// <example>
        ///     Aisling.HasCompletedQuest("Quest Name");
        ///     <code></code>
        /// </example>
        public bool HasCompletedQuest(string lpName)
        {
            return Quests.Any(i => i.Name == lpName && i.Completed);
        }

        /// <summary>
        ///     Forcefully Completes a Quest
        /// </summary>
        /// <param name="lpName">Name of the Quest to Complete</param>
        /// <example>
        ///     Aisling.CompleteQuest("Quest Name");
        ///     <code></code>
        /// </example>
        public void CompleteQuest(string lpName)
        {
            var obj = Quests.Find(i => i.Name == lpName);
            obj.Completed = true;
        }

        /// <summary>
        ///     Gets the quest.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Returns the Quest, or returns null if no Quest was retrieved.</returns>
        public Quest GetQuest(string name)
        {
            return Quests.FirstOrDefault(i => i.Name == name);
        }


        /// <summary>
        ///     Accepts the quest.
        /// </summary>
        /// <param name="lpQuest">The Quest to Accept.</param>
        /// <returns>
        ///     True if the quest was accepted, and the Aisling does not already have it accepted. Returns false if the Quest
        ///     has already been Accepted.
        /// </returns>
        /// <example>
        ///     <code>
        /// if (client.Aisling.AcceptQuest(quest))
        /// {
        /// //quest has been added to interpreter.
        /// }
        /// </code>
        /// </example>
        public bool AcceptQuest(Quest lpQuest)
        {
            lock (Quests)
            {
                if (!Quests.Any(i => i.Name == lpQuest.Name))
                {
                    Quests.Add(lpQuest);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Determines whether this instance is dead.
        /// </summary>
        /// <returns><c>true</c> if this instance is dead; otherwise, <c>false</c>.</returns>
        public bool IsDead()
        {
            var result = Flags.HasFlag(AislingFlags.Dead);

            return result;
        }

        /// <summary>
        ///     Determines whether this instance [can see ghosts].
        /// </summary>
        /// <returns><c>true</c> if this instance [can see ghosts]; otherwise, <c>false</c>.</returns>
        public bool CanSeeGhosts()
        {
            return IsDead();
        }

        /// <summary>
        ///     Determines whether this instance [can see hidden].
        /// </summary>
        /// <returns><c>true</c> if this instance [can see hidden]; otherwise, <c>false</c>.</returns>
        public bool CanSeeHidden()
        {
            return Flags.HasFlag(AislingFlags.SeeInvisible);
        }

        /// <summary>
        ///     Determines whether [has visited map] [the specified map identifier].
        /// </summary>
        /// <param name="mapId">The map identifier.</param>
        /// <returns><c>true</c> if [has visited map] [the specified map identifier]; otherwise, <c>false</c>.</returns>
        public bool HasVisitedMap(int mapId)
        {
            return DiscoveredMaps.Contains(mapId);
        }

        /// <summary>
        ///     Recovers this instance.
        /// </summary>
        public void Recover()
        {
            CurrentHp = MaximumHp;
            CurrentMp = MaximumMp;

            Client?.SendStats(StatusFlags.All);
        }


        /// <summary>
        ///     Determines whether [has in inventory] [the specified item].
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="count">The count.</param>
        /// <returns><c>true</c> if [has in inventory] [the specified item]; otherwise, <c>false</c>.</returns>
        public bool HasInInventory(string item, int count)
        {
            var template = ServerContext.GlobalItemTemplateCache[item];

            if (!ServerContext.GlobalItemTemplateCache.ContainsKey(item))
                return false;

            if (template != null) return Inventory.Has(template) >= count;

            return false;
        }

        /// <summary>
        ///     Determines whether the specified item is wearing.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if the specified item is wearing; otherwise, <c>false</c>.</returns>
        public bool IsWearing(string item)
        {
            return EquipmentManager.Equipment.Any(i => i.Value != null && i.Value.Item.Template.Name == item);
        }

        /// <summary>
        ///     Goes the home.
        /// </summary>
        public void GoHome()
        {
            var DestinationMap = ServerContext.Config.TransitionZone;

            if (ServerContext.GlobalMapCache.ContainsKey(DestinationMap))
            {
                var targetMap = ServerContext.GlobalMapCache[DestinationMap];

                Client.LeaveArea(true, true);
                Client.Aisling.XPos = ServerContext.Config.TransitionPointX;
                Client.Aisling.YPos = ServerContext.Config.TransitionPointY;
                Client.Aisling.CurrentMapId = DestinationMap;
                Client.EnterArea();
                Client.Refresh();
            }

            Client.CloseDialog();
        }

        /// <summary>
        ///     Determines whether [has mana for] [the specified spell].
        /// </summary>
        /// <param name="spell">The spell.</param>
        /// <returns>Aisling.</returns>
        public Aisling HasManaFor(Spell spell)
        {
            if (CurrentMp >= spell.Template.ManaCost)
                return this;
            Client.SendMessage(0x02, ServerContext.Config.NoManaMessage);

            return null;
        }

        /// <summary>
        ///     Updates the stats.
        /// </summary>
        /// <param name="lpSpell">The lp spell.</param>
        /// <returns>Aisling.</returns>
        public Aisling UpdateStats(Spell lpSpell)
        {
            Client.SendStats(StatusFlags.All);
            return this;
        }

        /// <summary>
        ///     Trains the spell.
        /// </summary>
        /// <param name="lpSpell">The lp spell.</param>
        /// <returns>Aisling.</returns>
        public Aisling TrainSpell(Spell lpSpell)
        {
            Client.TrainSpell(lpSpell);
            return this;
        }

        /// <summary>
        ///     Gives the health.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="value">The value.</param>
        /// <returns>Aisling.</returns>
        public Aisling GiveHealth(Sprite target, int value)
        {
            target.CurrentHp += value;

            if (target.CurrentHp > target.MaximumHp) target.CurrentHp = target.MaximumHp;

            return this;
        }

        /// <summary>
        ///     Casts the specified spell.
        /// </summary>
        /// <param name="spell">The spell.</param>
        /// <param name="target">The target.</param>
        /// <param name="actionSpeed">The action speed.</param>
        /// <returns>Aisling.</returns>
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

        /// <summary>
        ///     Casts the spell.
        /// </summary>
        /// <param name="spell">The spell.</param>
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

                var target = GetObject(Map, i => i.Serial == info.Target, Get.Monsters | Get.Aislings | Get.Mundanes);
                spell.InUse = true;


                if (spell.Script != null)
                {
                    if (target != null)
                    {
                        if (target is Aisling tobj)
                        {
                            spell.Script.OnUse(this, target as Aisling);                            
                        }

                        if (target is Monster aobj)
                        {
                            spell.Script.OnUse(this, aobj);
                        }

                        if (target is Mundane)
                            spell.Script.OnUse(this, target as Mundane);
                    }
                    else
                    {
                        spell.Script.OnUse(this, this);
                    }
                }
            }

            spell.NextAvailableUse = DateTime.UtcNow.AddSeconds(info.SpellLines > 0 ? 1 : 0.2);
            spell.InUse = false;

            if (spell.Template.Cooldown > 0)
            {
                Client.Send(new ServerFormat3F((byte)0,
                    spell.Slot,
                    spell.Template.Cooldown));
            }

        }

        /// <summary>
        ///     Destroys the reactor.
        /// </summary>
        /// <param name="Actor">The actor.</param>
        public void DestroyReactor(Reactor Actor)
        {
            if (Reactions.ContainsKey(Actor.Name))
                Reactions.Remove(Actor.Name);

            ActiveReactor = null;
            ReactorActive = false;
        }

        /// <summary>
        ///     Determines whether the specified value has killed.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="number">The number.</param>
        /// <returns><c>true</c> if the specified value has killed; otherwise, <c>false</c>.</returns>
        public bool HasKilled(string value, int number)
        {
            if (MonsterKillCounters.ContainsKey(value)) return MonsterKillCounters[value] >= number;

            return false;
        }

        /// <summary>
        ///     Creates this instance.
        /// </summary>
        /// <returns>Aisling.</returns>
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
                XPos = ServerContext.Config.StartingPosition.X,
                YPos = ServerContext.Config.StartingPosition.Y,
                Nation = (byte) randomFraction,
                AnimalForm = AnimalForm.None,                
            };

            Skill.GiveTo(result, "Assail", 1);
            Spell.GiveTo(result, "Create Item", 1);
            Spell.GiveTo(result, "Gem Polishing", 1);


            foreach (var temp in ServerContext.GlobalSpellTemplateCache)
            {
                Spell.GiveTo(result, temp.Value.Name);
            }

            foreach (var temp in ServerContext.GlobalSkillTemplateCache)
            {
                Skill.GiveTo(result, temp.Value.Name);
            }


            if (DateTime.UtcNow.Year <= 2020)
                result.LegendBook.AddLegend(new Legend.LegendItem
                {
                    Category = "Event",
                    Color = (byte) LegendColor.DarkPurple,
                    Icon = (byte) LegendIcon.Victory,
                    Value = "Aisling Age of Aquarius"
                });

            if (result.Nation == 1)
                result.LegendBook.AddLegend(new Legend.LegendItem
                {
                    Category = "Event",
                    Color = (byte) LegendColor.Orange,
                    Icon = (byte) LegendIcon.Community,
                    Value = "Lorule Citizen"
                });
            else if (result.Nation == 2)
                result.LegendBook.AddLegend(new Legend.LegendItem
                {
                    Category = "Event",
                    Color = (byte) LegendColor.LightGreen,
                    Icon = (byte) LegendIcon.Community,
                    Value = "Lividia Citizen"
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

        /// <summary>
        ///     Removes Aisling, Sends Remove packet to nearby aislings
        ///     and removes itself from the ObjectManager.
        /// </summary>
        /// <param name="update">if set to <c>true</c> [update].</param>
        /// <param name="delete">if set to <c>true</c> [delete].</param>
        public void Remove(bool update = false, bool delete = true)
        {
            if (Map != null)
                Map.Update(XPos, YPos);

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

        /// <summary>
        ///     Determines whether the specified scope has skill.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scope">The scope.</param>
        /// <returns><c>true</c> if the specified scope has skill; otherwise, <c>false</c>.</returns>
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
        ///     Gets the assails.
        /// </summary>
        /// <returns>Skill[].</returns>
        public Skill[] GetAssails()
        {
            return SkillBook.Get(i => i != null && i.Template != null
                                                && i.Template.Type == SkillScope.Assail).ToArray();
        }

        /// <summary>
        ///     Updates the stats.
        /// </summary>
        public void UpdateStats()
        {
            Client?.SendStats(StatusFlags.All);
        }


        /// <summary>
        ///     Casts the death.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool CastDeath()
        {
            if (!Client.Aisling.Flags.HasFlag(AislingFlags.Dead))
            {
                LastMapId = CurrentMapId;
                LastPosition = Position;

                Client.CloseDialog();
                Client.Ghost();


                return true;
            }

            return false;
        }

        /// <summary>
        ///     Sends to hell.
        /// </summary>
        public void SendToHell()
        {
            if (!ServerContext.GlobalMapCache.ContainsKey(ServerContext.Config.DeathMap))
                return;

            if (CurrentMapId == ServerContext.Config.DeathMap)
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
            XPos = 21;
            YPos = 21;
            Direction = 0;
            Client.Aisling.CurrentMapId = ServerContext.Config.DeathMap;
            Client.EnterArea();

            UpdateStats();
        }

        /// <summary>
        ///     Cancels the exchange.
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
                if (item.GiveTo(trader))
                {
                }

            foreach (var item in itemsA)
                if (item.GiveTo(this))
                {
                }


            GoldPoints += goldA;
            trader.GoldPoints += goldB;


            if (trader.GoldPoints > ServerContext.Config.MaxCarryGold)
                trader.GoldPoints = ServerContext.Config.MaxCarryGold;
            if (GoldPoints > ServerContext.Config.MaxCarryGold)
                GoldPoints = ServerContext.Config.MaxCarryGold;

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

        /// <summary>
        ///     Finishes the exchange.
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
                if (item.GiveTo(this))
                {
                }

            foreach (var item in itemsA)
                if (item.GiveTo(trader))
                {
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

        /// <summary>
        ///     Reacteds the with.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool ReactedWith(string str)
        {
            return Reactions.ContainsKey(str);
        }

        /// <summary>
        ///     Revives the in front.
        /// </summary>
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

        /// <summary>
        ///     Gives the gold.
        /// </summary>
        /// <param name="offer">The offer.</param>
        /// <param name="SendClientUpdate">if set to <c>true</c> [send client update].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool GiveGold(int offer, bool SendClientUpdate = true)
        {
            if (GoldPoints + offer < ServerContext.Config.MaxCarryGold)
            {
                GoldPoints += offer;
                return true;
            }

            if (SendClientUpdate) Client?.SendStats(StatusFlags.StructC);

            return false;
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Username;
        }

        #region Reactor Stuff

        /// <summary>
        ///     Gets or sets a value indicating whether [reactor active].
        /// </summary>
        /// <value><c>true</c> if [reactor active]; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool ReactorActive { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance can react.
        /// </summary>
        /// <value><c>true</c> if this instance can react; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool CanReact { get; set; }

        /// <summary>
        ///     Gets or sets the active sequence.
        /// </summary>
        /// <value>The active sequence.</value>
        [JsonIgnore]
        public DialogSequence ActiveSequence { get; set; }

        /// <summary>
        ///     Gets or sets the animal form.
        /// </summary>
        /// <value>The animal form.</value>
        public AnimalForm AnimalForm { get; set; }

        /// <summary>
        ///     Used for Internal Server Authentication
        /// </summary>
        /// <value>The redirect.</value>
        public Redirect Redirect { get; set; }

        #endregion
    }
}