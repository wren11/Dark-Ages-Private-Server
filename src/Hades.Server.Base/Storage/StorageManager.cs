#region

using ComponentAce.Compression.Libs.zlib;
using Darkages.Common;
using Darkages.Compression;
using Darkages.IO;
using Darkages.Network;
using Darkages.Network.ClientFormats;
using Darkages.Network.Game;
using Darkages.Network.Game.Components;
using Darkages.Network.Login;
using Darkages.Network.Object;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Security;
using Darkages.Storage.locales.Buffs;
using Darkages.Storage.locales.debuffs;
using Darkages.Systems.CLI;
using Darkages.Systems.Loot;
using Darkages.Systems.Loot.Interfaces;
using Darkages.Systems.Loot.Modifiers;
using Darkages.Templates;
using Darkages.Types;
using Darkages.Types.Templates;
using Lorule.Editor;
using Lorule.GameServer;
using MenuInterpreter;
using MenuInterpreter.Parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Pyratron.Frameworks.Commands.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using Answer = MenuInterpreter.Answer;

#endregion

namespace Darkages.Storage
{
    public class StorageManager
    {
        public static AislingStorage AislingBucket
            = new AislingStorage();

        public static AreaStorage AreaBucket
            = new AreaStorage();

        public static TemplateStorage<ItemTemplate> ItemBucket = new TemplateStorage<ItemTemplate>();

        public static TemplateStorage<MonsterTemplate> MonsterBucket = new TemplateStorage<MonsterTemplate>();

        public static TemplateStorage<MundaneTemplate> MundaneBucket = new TemplateStorage<MundaneTemplate>();

        public static TemplateStorage<NationTemplate> NationBucket = new TemplateStorage<NationTemplate>();

        public static TemplateStorage<PopupTemplate> PopupBucket = new TemplateStorage<PopupTemplate>();

        public static TemplateStorage<Reactor> ReactorBucket = new TemplateStorage<Reactor>();

        public static TemplateStorage<ServerTemplate> ServerArgBucket = new TemplateStorage<ServerTemplate>();

        public static KnownTypesBinder HadesTypesBinder = new KnownTypesBinder();

        static StorageManager()
        {
            HadesTypesBinder.KnownTypes = new List<Type>
            {
                typeof(Answer),
                typeof(CheckpointHandlerArgs),
                typeof(QuestHanderArgs),
                typeof(CheckpointMenuItem),
                typeof(Constants),
                typeof(HistoryItem),
                typeof(Interpreter),
                typeof(MenuItem),
                typeof(MenuItemType),
                typeof(IMenuParser),
                typeof(Answer),
                typeof(Checkpoint),
                typeof(Link),
                typeof(Menu),
                typeof(Option),
                typeof(ParseResult),
                typeof(QuestEvent),
                typeof(Sequence),
                typeof(Start),
                typeof(Step),
                typeof(YamlMenuParser),
                typeof(Argument),
                typeof(Command),
                typeof(CommandExtensions),
                typeof(CommandParser),
                typeof(IArguable),
                typeof(LoruleOptions),
                typeof(EditorOptions),
                typeof(Adler32),
                typeof(Deflate),
                typeof(InfBlocks),
                typeof(InfCodes),
                typeof(Inflate),
                typeof(InfTree),
                typeof(StaticTree),
                typeof(SupportClass),
                typeof(Tree),
                typeof(ZInputStream),
                typeof(zlibConst),
                typeof(ZOutputStream),
                typeof(ZStream),
                typeof(ZStreamException),
                typeof(IServerConstants),
                typeof(GameSetting),
                typeof(ServerConstants),
                typeof(ServerContext),
                typeof(IServerContext),
                typeof(KillRecord),
                typeof(Aisling),
                typeof(Area),
                typeof(EphemeralReactor),
                typeof(Map),
                typeof(PartyMember),
                typeof(PortalSession),
                typeof(TileGrid),
                typeof(WarpType),
                typeof(Types.Common),
                typeof(Epic),
                typeof(Forsaken),
                typeof(Godly),
                typeof(ItemTemplate),
                typeof(ItemUpgrade),
                typeof(Legendary),
                typeof(Mythical),
                typeof(Rare),
                typeof(Uncommon),
                typeof(MonsterTemplate),
                typeof(ViewQualifer),
                typeof(MundaneTemplate),
                typeof(NationTemplate),
                typeof(TriggerType),
                typeof(ItemClickPopup),
                typeof(ItemDropPopup),
                typeof(ItemPickupPopup),
                typeof(Popup),
                typeof(PopupTemplate),
                typeof(UserClickPopup),
                typeof(UserWalkPopup),
                typeof(SkillTemplate),
                typeof(SpellTemplate),
                typeof(WarpTemplate),
                typeof(WorldMapTemplate),
                typeof(ActivationTrigger),
                typeof(ActivityStatus),
                typeof(AislingFlags),
                typeof(AnimalForm),
                typeof(AttackModifier),
                typeof(DamageModifier),
                typeof(Bank),
                typeof(Board),
                typeof(BoardDescriptors),
                typeof(BoardList),
                typeof(ForumCallback),
                typeof(BodySprite),
                typeof(Buff),
                typeof(CastInfo),
                typeof(Class),
                typeof(ClassStage),
                typeof(ClientGameSettings),
                typeof(CursedSachel),
                typeof(Debuff),
                typeof(Dialog),
                typeof(DialogSequence),
                typeof(DialogSession),
                typeof(Direction),
                typeof(ElementManager),
                typeof(ElementQualifer),
                typeof(EquipmentManager),
                typeof(EquipmentSlot),
                typeof(ExchangeSession),
                typeof(Gender),
                typeof(GroupStatus),
                typeof(Inventory),
                typeof(ISprite),
                typeof(Item),
                typeof(ItemColor),
                typeof(ItemFlags),
                typeof(PendingSell),
                typeof(EquipSlot),
                typeof(ItemSlots),
                typeof(ItemPredicate),
                typeof(LearningPredicate),
                typeof(Legend),
                typeof(LegendColor),
                typeof(LegendIcon),
                typeof(LootQualifer),
                typeof(MapFlags),
                typeof(MapKeyPair),
                typeof(Metafile),
                typeof(MetafileCollection),
                typeof(Node),
                typeof(MetafileManager),
                typeof(MetafileNode),
                typeof(Money),
                typeof(MoneySprites),
                typeof(Monster),
                typeof(MonsterDamageType),
                typeof(MoodQualifer),
                typeof(MServer),
                typeof(MServerTable),
                typeof(Mundane),
                typeof(Notification),
                typeof(Operator),
                typeof(Party),
                typeof(PathQualifer),
                typeof(Pet),
                typeof(Position),
                typeof(PostQualifer),
                typeof(PrimaryStat),
                typeof(QuestDelegate),
                typeof(PlayerAttr),
                typeof(QuestType),
                typeof(AttrReward),
                typeof(Quest),
                typeof(QuestChain),
                typeof(QuestRequirement),
                typeof(Reactor),
                typeof(ReactorQualifer),
                typeof(Redirect),
                typeof(Scope),
                typeof(Skill),
                typeof(SkillBook),
                typeof(SkillModifiers),
                typeof(Pane),
                typeof(SkillScope),
                typeof(SpawnQualifer),
                typeof(Spell),
                typeof(SpellBook),
                typeof(SpellOperator),
                typeof(Sprite),
                typeof(Stat),
                typeof(StatusFlags),
                typeof(StatusOperator),
                typeof(IEphermeral),
                typeof(Summon),
                typeof(TargetQualifiers),
                typeof(Template),
                typeof(Tier),
                typeof(TileContent),
                typeof(Trap),
                typeof(Warp),
                typeof(WorldPortal),
                typeof(ServerTemplate),
                typeof(Politics),
                typeof(LootDropper),
                typeof(LootModifierSet),
                typeof(LootTable),
                typeof(BaseModifier),
                typeof(Operation),
                typeof(NumericModifier),
                typeof(StringModifier),
                typeof(ILootDefinition),
                typeof(ILootDropper),
                typeof(ILootTable),
                typeof(IModifier),
                typeof(IModifierSet),
                typeof(IWeighable),
                typeof(Commander),
                typeof(AislingStorage),
                typeof(AreaStorage),
                typeof(StorageManager),
                typeof(WarpStorage),
                typeof(debuff_ardcradh),
                typeof(debuff_beagcradh),
                typeof(debuff_beagsuain),
                typeof(debuff_blind),
                typeof(debuff_cradh),
                typeof(debuff_cursed),
                typeof(debuff_fasnadur),
                typeof(debuff_fasspoirad),
                typeof(debuff_frozen),
                typeof(debuff_hurricane),
                typeof(debuff_morcradh),
                typeof(debuff_morfasnadur),
                typeof(Debuff_poison),
                typeof(debuff_reeping),
                typeof(debuff_sleep),
                typeof(BuffAite),
                typeof(buff_armachd),
                typeof(buff_clawfist),
                typeof(buff_dion),
                typeof(buff_hide),
                typeof(buff_mordion),
                typeof(buff_spell_reflect),
                typeof(SecurityParameters),
                typeof(SecurityProvider),
                typeof(AreaScript),
                typeof(DamageFormulaScript),
                typeof(ElementFormulaScript),
                typeof(FormulaScript),
                typeof(GlobalScript),
                typeof(ItemScript),
                typeof(MonsterCreateScript),
                typeof(MonsterScript),
                typeof(MundaneScript),
                typeof(ReactorScript),
                typeof(RewardScript),
                typeof(IScriptBase),
                typeof(IUseable),
                typeof(IUseableTarget),
                typeof(ScriptManager),
                typeof(ScriptAttribute),
                typeof(SkillScript),
                typeof(SpellScript),
                typeof(WeaponScript),
                typeof(BufferReader),
                typeof(BufferWriter),
                typeof(Crc16Provider),
                typeof(Crc32Provider),
                typeof(IPAttribute),
                typeof(NetworkClient),
                typeof(NetworkPacket),
                typeof(NetworkPacketReader),
                typeof(NetworkPacketWriter),
                typeof(NetworkSocket),
                typeof(IDialogData),
                typeof(BankingData),
                typeof(ItemSellData),
                typeof(ItemShopData),
                typeof(OptionsData),
                typeof(OptionsDataItem),
                typeof(OptionsPlusArgsData),
                typeof(SkillAcquireData),
                typeof(SkillForfeitData),
                typeof(SpellAcquireData),
                typeof(SpellForfeitData),
                typeof(TextInputData),
                typeof(WithdrawBankData),
                typeof(ReactorInputSequence),
                typeof(ReactorSequence),
                typeof(IObjectManager),
                typeof(ObjectManager),
                typeof(ObjectService),
                typeof(LoginClient),
                typeof(LoginServer),
                typeof(GameServerTimer),
                typeof(GameClient),
                typeof(GameServer),
                typeof(GameServerComponent),
                typeof(Network.Game.Lorule),
                typeof(ScriptGlobals),
                typeof(IGameClient),
                typeof(MessageComponent),
                typeof(MonolithComponent),
                typeof(MundaneComponent),
                typeof(ObjectComponent),
                typeof(PingComponent),
                typeof(SaveComponent),
                typeof(CompressableObject),
                typeof(CompressionProvider),
                typeof(Extensions),
                typeof(Generator),
                typeof(Interpreter.CheckpointHandler),
                typeof(Interpreter.MovedToNextStepHandler),
                typeof(CommandParser.ParseErrorHandler),
                typeof(SpellTemplate.SpellUseType),
                typeof(ElementManager.Element),
                typeof(Item.Variance),
                typeof(Legend.LegendItem),
                typeof(Position.TileContentPosition),
                typeof(SpellOperator.SpellOperatorPolicy),
                typeof(SpellOperator.SpellOperatorScope),
                typeof(KnownTypesBinder),
                typeof(ServerFormat36.ClassType),
                typeof(ServerFormat36.ListColor),
                typeof(ServerFormat36.StatusIcon),
                typeof(ObjectManager.Get),
                typeof(ClientFormat0E.MsgType),
                typeof(PostFormat),
                typeof(BoardDescriptors),
                typeof(BoardList),
                typeof(Board),
                typeof(KillRecord)
            };
        }

        public class KnownTypesBinder : ISerializationBinder
        {
            public IList<Type> KnownTypes { get; set; }

            public Type BindToType(string assemblyName, string typeName)
            {
                return KnownTypes.Distinct().SingleOrDefault(t => t.Name == typeName);
            }

            public void BindToName(Type serializedType, out string assemblyName, out string typeName)
            {
                assemblyName = null;
                typeName = serializedType.Name;
            }
        }

        public static JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            SerializationBinder = HadesTypesBinder,
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };


        public static string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, Settings);
        }

        public static T Deserialize<T>(string data)
        {
            return (T)JsonConvert.DeserializeObject<T>(data, Settings);
        }


        public static TemplateStorage<SkillTemplate> SkillBucket = new TemplateStorage<SkillTemplate>();

        public static TemplateStorage<SpellTemplate> SpellBucket = new TemplateStorage<SpellTemplate>();

        public static WarpStorage WarpBucket = new WarpStorage();

        public static TemplateStorage<WorldMapTemplate> WorldMapBucket = new TemplateStorage<WorldMapTemplate>();

    }
}