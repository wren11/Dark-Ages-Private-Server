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
using Darkages.Network.Game;
using Darkages.Network.Login;
using Darkages.Network.Object;
using Darkages.Storage;
using Darkages.Storage.locales.Buffs;
using Darkages.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Darkages
{
    public class ServerContext : ObjectManager
    {
        internal static volatile object SyncObj = new object();

        public static int Errors, DefaultPort;
        public static bool Running, Paused;

        public static GameServer Game;
        public static LoginServer Lobby;
        public static ServerConstants Config;
        public static IPAddress Ipaddress => IPAddress.Parse(File.ReadAllText("server.tbl"));
        public static string StoragePath = @"..\..\..\LORULE_DATA";

        public static List<Redirect> GlobalRedirects = new List<Redirect>();

        public static List<Metafile> GlobalMetaCache = new List<Metafile>();

        public static Dictionary<int, Area> GlobalMapCache =
            new Dictionary<int, Area>();

        public static List<MonsterTemplate> GlobalMonsterTemplateCache =
            new List<MonsterTemplate>();

        public static Dictionary<string, SkillTemplate> GlobalSkillTemplateCache =
            new Dictionary<string, SkillTemplate>();

        public static Dictionary<string, SpellTemplate> GlobalSpellTemplateCache =
            new Dictionary<string, SpellTemplate>();

        public static Dictionary<string, ItemTemplate> GlobalItemTemplateCache =
            new Dictionary<string, ItemTemplate>();

        public static Dictionary<string, MundaneTemplate> GlobalMundaneTemplateCache =
            new Dictionary<string, MundaneTemplate>();

        public static List<WarpTemplate> GlobalWarpTemplateCache =
            new List<WarpTemplate>();

        public static Dictionary<int, WorldMapTemplate> GlobalWorldMapTemplateCache =
            new Dictionary<int, WorldMapTemplate>();

        public static Board[] Community = new Board[7];

        public static void LoadSkillTemplates()
        {
            StorageManager.SkillBucket.CacheFromStorage();
            logger.Trace("Skill Templates Loaded: {0}", GlobalSkillTemplateCache.Count);
        }

        public static void LoadSpellTemplates()
        {
            StorageManager.SpellBucket.CacheFromStorage();
            logger.Trace("Spell Templates Loaded: {0}", GlobalSpellTemplateCache.Count);
        }

        public static void LoadItemTemplates()
        {
            StorageManager.ItemBucket.CacheFromStorage();
            logger.Trace("Item Templates Loaded: {0}", GlobalItemTemplateCache.Count);
        }

        public static void LoadMonsterTemplates()
        {
            StorageManager.MonsterBucket.CacheFromStorage();
            logger.Trace("Monster Templates Loaded: {0}", GlobalMonsterTemplateCache.Count);
        }

        public static void LoadMundaneTemplates()
        {
            StorageManager.MundaneBucket.CacheFromStorage();
            logger.Trace("Mundane Templates Loaded: {0}", GlobalMundaneTemplateCache.Count);
        }

        public static void LoadWarpTemplates()
        {
            StorageManager.WarpBucket.CacheFromStorage();
            logger.Trace("Warp Templates Loaded: {0}", GlobalWarpTemplateCache.Count);
        }

        public static void LoadWorldMapTemplates()
        {
            StorageManager.WorldMapBucket.CacheFromStorage();
            logger.Trace("World Map Templates Loaded: {0}", GlobalWorldMapTemplateCache.Count);
        }

        public static void LoadMaps()
        {
            StorageManager.AreaBucket.CacheFromStorage();
            logger.Trace("Map Templates Loaded: {0}", GlobalMapCache.Count);
        }

        private static void StartServers()
        {
#if DEBUG
            ServerContext.Config.DebugMode = true;
#endif

            redo:
            {
                if (Errors > Config.ERRORCAP)
                    Process.GetCurrentProcess().Kill();

                try
                {
                    Lobby = new LoginServer(Config.ConnectionCapacity);
                    Lobby.Start(Config.LOGIN_PORT);

                    Game = new GameServer(Config.ConnectionCapacity);
                    Game.Start(DefaultPort);
                }
                catch (Exception)
                {
                    { ++DefaultPort; Errors++; }
                    goto redo;
                }
            }


            Running = true;
        }

        /// <summary>
        ///     EP
        /// </summary>
        public virtual void Start()
        {
            Startup();
        }

        public virtual void Shutdown()
        {
            Running = false;
            {
                EmptyCacheCollectors();

                Game?.Abort();
                Lobby?.Abort();

                Game = null;
                Lobby = null;
            }
        }

        public static void Startup()
        {
            logger.Warn(Config.SERVER_TITLE);
            {
                try
                {
                    LoadConstants();
                    LoadAndCacheStorage();
                    StartServers();
                }
                catch (Exception error)
                {
                    logger.Error(error, "Startup Error.");
                }
            }
            logger.Warn("{0} Online.", Config.SERVER_TITLE);
        }

        private static void EmptyCacheCollectors()
        {
            GlobalItemTemplateCache = new Dictionary<string, ItemTemplate>();
            GlobalMapCache = new Dictionary<int, Area>();
            GlobalMetaCache = new List<Metafile>();
            GlobalMonsterTemplateCache = new List<MonsterTemplate>();
            GlobalMundaneTemplateCache = new Dictionary<string, MundaneTemplate>();
            GlobalRedirects = new List<Redirect>();
            GlobalSkillTemplateCache = new Dictionary<string, SkillTemplate>();
            GlobalSpellTemplateCache = new Dictionary<string, SpellTemplate>();
            GlobalWarpTemplateCache = new List<WarpTemplate>();
            GlobalWorldMapTemplateCache = new Dictionary<int, WorldMapTemplate>();


        }

        public static void LoadConstants()
        {
            var _config_ = StorageManager.Load<ServerConstants>();

            if (_config_ == null)
            {
                logger.Trace("No config found. Generating defaults.");
                Config = new ServerConstants();
                StorageManager.Save(Config);
            }
            else
            {
                Config = StorageManager.Load<ServerConstants>();
            }

            InitFromConfig();
        }

        public static void InitFromConfig()
        {
            DefaultPort = Config.SERVER_PORT;
            {
                if (!Directory.Exists(StoragePath))
                    Directory.CreateDirectory(StoragePath);
            }
        }

        public static void LoadMetaDatabase()
        {
            logger.Trace("Loading Meta Database");
            {
                GlobalMetaCache.AddRange(MetafileManager.GetMetafiles());
            }
            logger.Trace("Building Meta Cache: {0} loaded.", GlobalMetaCache.Count);
        }

        public static void SaveCommunityAssets()
        {
            List<Board> tmp;

            lock (Community)
            {
                tmp = new List<Board>(Community);
            }

            foreach (var asset in tmp)
            {
                asset.Save();
            }
        }

        public static void CacheCommunityAssets()
        {
            if (Community != null)
            {
                Community =
                    Board.CacheFromStorage()?.OrderBy(i => i.Index).ToArray();
            }
        }

        public static async void LoadAndCacheStorage()
        {
            Paused = true;
            Paused = await Task.Run(() =>
            {
                EmptyCacheCollectors();
                lock (SyncObj)
                {
                    LoadMaps();
                    LoadSkillTemplates();
                    LoadSpellTemplates();
                    LoadItemTemplates();
                    LoadMonsterTemplates();
                    LoadMundaneTemplates();
                    LoadWarpTemplates();
                    LoadWorldMapTemplates();
                    LoadMetaDatabase();
                    CacheCommunityAssets();
                }


                GlobalItemTemplateCache["Hy-Brasyl Battle Axe"].Class = Class.Warrior;


                GlobalItemTemplateCache["Earth Necklace"] = new ItemTemplate()
                {
                    CanStack = false,
                    Class = Class.Peasant,
                    CarryWeight = 1,
                    Color = ItemColor.defaultgreen,
                    OffenseElement = ElementManager.Element.Earth,
                    DisplayImage = 0x80C5,
                    Image = 0xC5,
                    LevelRequired = 6,
                    DropRate = 0.50,
                    Value = 10000,
                    MaxDurability = 5000,
                    Name = "Earth Necklace",
                    NpcKey = "Precious Jewels",
                    Flags = ItemFlags.Bankable | ItemFlags.Elemental | ItemFlags.Equipable | ItemFlags.Dropable | ItemFlags.Repairable | ItemFlags.Sellable | ItemFlags.Upgradeable,
                    Gender = Gender.Both,
                    EquipmentSlot = ItemSlots.Necklace,
                    ScriptName = "Necklace"
                };

                GlobalItemTemplateCache["Fire Necklace"] = new ItemTemplate()
                {
                    CanStack = false,
                    Class = Class.Peasant,
                    CarryWeight = 1,
                    Color = ItemColor.defaultgreen,
                    OffenseElement = ElementManager.Element.Fire,
                    DisplayImage = 0x80CD,
                    Image = 0xCD,
                    LevelRequired = 6,
                    DropRate = 0.50,
                    Value = 10000,
                    MaxDurability = 5000,
                    Name = "Fire Necklace",
                    NpcKey = "Precious Jewels",
                    Flags = ItemFlags.Bankable | ItemFlags.Elemental | ItemFlags.Equipable | ItemFlags.Dropable | ItemFlags.Repairable | ItemFlags.Sellable | ItemFlags.Upgradeable,
                    Gender = Gender.Both,
                    EquipmentSlot = ItemSlots.Necklace,
                    ScriptName = "Necklace"
                };

                GlobalItemTemplateCache["Wind Necklace"] = new ItemTemplate()
                {
                    CanStack = false,
                    Class = Class.Peasant,
                    CarryWeight = 1,
                    Color = ItemColor.defaultgreen,
                    OffenseElement = ElementManager.Element.Wind,
                    DisplayImage = 0x80C6,
                    Image = 0xC6,
                    LevelRequired = 6,
                    DropRate = 0.50,
                    Value = 10000,
                    MaxDurability = 5000,
                    Name = "Wind Necklace",
                    NpcKey = "Precious Jewels",
                    Flags = ItemFlags.Bankable | ItemFlags.Elemental | ItemFlags.Equipable | ItemFlags.Dropable | ItemFlags.Repairable | ItemFlags.Sellable | ItemFlags.Upgradeable,
                    Gender = Gender.Both,
                    EquipmentSlot = ItemSlots.Necklace,
                    ScriptName = "Necklace"
                };


                GlobalItemTemplateCache["Sea Necklace"] = new ItemTemplate()
                {
                    CanStack = false,
                    Class = Class.Peasant,
                    CarryWeight = 1,
                    Color = ItemColor.defaultgreen,
                    OffenseElement = ElementManager.Element.Water,
                    DisplayImage = 0x80C7,
                    Image = 0xC7,
                    LevelRequired = 6,
                    DropRate = 0.50,
                    Value = 10000,
                    MaxDurability = 5000,
                    Name = "Sea Necklace",
                    NpcKey = "Precious Jewels",
                    Flags = ItemFlags.Bankable | ItemFlags.Elemental | ItemFlags.Equipable | ItemFlags.Dropable | ItemFlags.Repairable | ItemFlags.Sellable | ItemFlags.Upgradeable,
                    Gender = Gender.Both,
                    EquipmentSlot = ItemSlots.Necklace,
                    ScriptName = "Necklace"
                };


                GlobalItemTemplateCache["Dark Necklace"] = new ItemTemplate()
                {
                    CanStack = false,
                    Class = Class.Peasant,
                    CarryWeight = 1,
                    Color = ItemColor.defaultgreen,
                    OffenseElement = ElementManager.Element.Dark,
                    DisplayImage = 0x80CA,
                    Image = 0xCA,
                    LevelRequired = 6,
                    DropRate = 0.20,
                    Value = 1000000,
                    MaxDurability = 5000,
                    Name = "Dark Necklace",
                    NpcKey = "Precious Jewels",
                    Flags = ItemFlags.Bankable | ItemFlags.Elemental | ItemFlags.Equipable | ItemFlags.Dropable | ItemFlags.Repairable | ItemFlags.Sellable | ItemFlags.Upgradeable,
                    Gender = Gender.Both,
                    EquipmentSlot = ItemSlots.Necklace,
                    ScriptName = "Necklace"
                };

                GlobalItemTemplateCache["Elemental Band"] = new ItemTemplate()
                {
                    CanStack = false,
                    Class = Class.Peasant,
                    CarryWeight = 1,
                    Color = ItemColor.defaultgreen,
                    OffenseElement = ElementManager.Element.Random,
                    DisplayImage = 0x8820,
                    Image = 0x0820,
                    LevelRequired = 6,
                    DropRate = 0.02,
                    Value = 100000000,
                    MaxDurability = 5000,
                    Name = "Elemental Band",
                    NpcKey = "Precious Jewels",
                    Flags = ItemFlags.Bankable | ItemFlags.Elemental | ItemFlags.Equipable | ItemFlags.Dropable | ItemFlags.Repairable | ItemFlags.Sellable | ItemFlags.Upgradeable,
                    Gender = Gender.Both,
                    EquipmentSlot = ItemSlots.Necklace,
                    ScriptName = "Necklace",
                };

                GlobalItemTemplateCache["Black Pearl Ring"] = new ItemTemplate()
                {
                    CanStack = false,
                    Class = Class.Peasant,
                    CarryWeight = 1,
                    AcModifer = new StatusOperator(StatusOperator.Operator.Remove, 5),
                    StrModifer = new StatusOperator(StatusOperator.Operator.Add, 6),
                    ConModifer = new StatusOperator(StatusOperator.Operator.Add, 6),
                    DexModifer = new StatusOperator(StatusOperator.Operator.Add, 6),
                    IntModifer = new StatusOperator(StatusOperator.Operator.Add, 6),
                    WisModifer = new StatusOperator(StatusOperator.Operator.Add, 6),
                    HitModifer = new StatusOperator(StatusOperator.Operator.Add, 5),
                    DmgModifer = new StatusOperator(StatusOperator.Operator.Add, 5),
                    HealthModifer = new StatusOperator(StatusOperator.Operator.Add, 2000),
                    ManaModifer = new StatusOperator(StatusOperator.Operator.Add, 2000),
                    DisplayImage = 0x829E,
                    Image = 0x029E,
                    LevelRequired = 99,
                    DropRate = 0.05,
                    Value = 20000000,
                    MaxDurability = 5000,
                    Name = "Black Pearl Ring",
                    NpcKey = "Precious Jewels",
                    Flags = ItemFlags.Bankable | ItemFlags.Elemental | ItemFlags.Equipable | ItemFlags.Dropable | ItemFlags.Repairable | ItemFlags.Sellable | ItemFlags.Upgradeable,
                    Gender = Gender.Both,
                    EquipmentSlot = ItemSlots.LHand,
                    ScriptName = "Ring"
                };


                GlobalSpellTemplateCache["ao beag cradh"]
                 = new SpellTemplate()
                 {
                     Animation = 39,
                     BaseLines = 2,
                     Icon = 23,
                     LevelRate = 0.20,
                     ManaCost = 40,
                     MaxLevel = 100,
                     Name = "ao beag cradh",
                     NpcKey = "Etaen",
                     MinLines = 0,
                     Pane = Pane.Spells,
                     TargetType = SpellTemplate.SpellUseType.ChooseTarget,
                     MaxLines = 3,
                     TierLevel = Tier.Tier1,
                     Sound = 8,
                     ScriptKey = "ao beag cradh"
                 };

                GlobalSpellTemplateCache["ao suain"]
                 = new SpellTemplate()
                 {
                     Animation = 2,
                     BaseLines = 2,
                     Icon = 51,
                     LevelRate = 0.20,
                     ManaCost = 40,
                     MaxLevel = 100,
                     Name = "ao suain",
                     NpcKey = "Etaen",
                     MinLines = 0,
                     Pane = Pane.Spells,
                     TargetType = SpellTemplate.SpellUseType.ChooseTarget,
                     MaxLines = 3,
                     TierLevel = Tier.Tier1,
                     Sound = 8,
                     ScriptKey = "ao suain"
                 };

                GlobalSpellTemplateCache["dion"].Buff = new buff_dion();
                GlobalSpellTemplateCache["mor dion"].Buff = new buff_mordion();


                return false;
            });

        }
    }
}
