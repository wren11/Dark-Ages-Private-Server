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
using Darkages.Storage.locales.debuffs;
using Darkages.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Darkages
{
    public class ServerContext : ObjectManager
    {
        public static object SyncObj = new object();

        public static int Errors, DefaultPort;
        public static bool Running, Paused;

        public static GameServer Game;
        public static LoginServer Lobby;
        public static ServerConstants Config;
        public static IPAddress Ipaddress => IPAddress.Parse(File.ReadAllText("server.tbl"));
        public static string StoragePath = @"..\..\..\LORULE_DATA";
        public static string ScriptOEMPath = @"..\..\..\Darkages.Server\Assets\locales";

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

        public static void UpdateLocales()
        {
            var src = Path.GetFullPath(ScriptOEMPath);
            var dst = Path.GetFullPath(StoragePath);

            CopyFolderContents(src, dst);
        }

        private static bool CopyFolderContents(string SourcePath, string DestinationPath)
        {
            SourcePath = SourcePath.EndsWith(@"\") ? SourcePath : SourcePath + @"\";
            DestinationPath = DestinationPath.EndsWith(@"\") ? DestinationPath : DestinationPath + @"\";

            try
            {
                if (Directory.Exists(SourcePath))
                {
                    if (Directory.Exists(DestinationPath) == false)
                    {
                        Directory.CreateDirectory(DestinationPath);
                    }

                    foreach (string file in Directory.GetFiles(SourcePath))
                    {

                        var a = new FileInfo(file);
                        var b = new FileInfo(Path.Combine(DestinationPath, a.Name));

                        if (b.Exists)
                        {
                            if (a.LastWriteTimeUtc != b.LastWriteTimeUtc && a.Length != b.Length)
                            {
                                a.CopyTo(string.Format(@"{0}\{1}", DestinationPath, a.Name), true);
                            }
                        }
                        else
                        {
                            a.CopyTo(string.Format(@"{0}\{1}", DestinationPath, a.Name), true);
                        }
                    }

                    foreach (string drs in Directory.GetDirectories(SourcePath))
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(drs);
                        if (CopyFolderContents(drs, DestinationPath + directoryInfo.Name) == false)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
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
                var mfs = MetafileManager.GetMetafiles();

                if (mfs.Count > 0)
                {
                    GlobalMetaCache.AddRange(mfs);
                }
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
                    UpdateLocales();
                    LoadMaps();
                    LoadSkillTemplates();
                    LoadSpellTemplates();
                    LoadItemTemplates();
                    LoadMonsterTemplates();
                    LoadMundaneTemplates();
                    LoadWarpTemplates();
                    LoadWorldMapTemplates();
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
                     NpcKey = "etaen",
                     MinLines = 0,
                     Pane = Pane.Spells,
                     TargetType = SpellTemplate.SpellUseType.ChooseTarget,
                     MaxLines = 3,
                     TierLevel = Tier.Tier1,
                     Sound = 8,
                     ScriptKey = "ao beag cradh"
                 };


                GlobalSpellTemplateCache["ao cradh"]
                 = new SpellTemplate()
                 {
                     Animation = 38,
                     BaseLines = 2,
                     Icon = 79,
                     LevelRate = 0.20,
                     ManaCost = 100,
                     MaxLevel = 100,
                     Name = "ao cradh",
                     NpcKey = "etaen",
                     MinLines = 0,
                     Pane = Pane.Spells,
                     TargetType = SpellTemplate.SpellUseType.ChooseTarget,
                     MaxLines = 3,
                     TierLevel = Tier.Tier1,
                     Sound = 8,
                     ScriptKey = "ao cradh"
                 };

                GlobalSpellTemplateCache["ao mor cradh"]
                 = new SpellTemplate()
                 {
                     Animation = 8,
                     BaseLines = 2,
                     Icon = 80,
                     LevelRate = 0.20,
                     ManaCost = 150,
                     MaxLevel = 100,
                     Name = "ao mor cradh",
                     NpcKey = "dar",
                     MinLines = 0,
                     Pane = Pane.Spells,
                     TargetType = SpellTemplate.SpellUseType.ChooseTarget,
                     MaxLines = 3,
                     TierLevel = Tier.Tier1,
                     Sound = 8,
                     ScriptKey = "ao mor cradh"
                 };

                GlobalSpellTemplateCache["ao ard cradh"]
                 = new SpellTemplate()
                 {
                     Animation = 37,
                     BaseLines = 2,
                     Icon = 81,
                     LevelRate = 0.30,
                     ManaCost = 240,
                     MaxLevel = 100,
                     Name = "ao ard cradh",
                     NpcKey = "dar",
                     MinLines = 0,
                     Pane = Pane.Spells,
                     TargetType = SpellTemplate.SpellUseType.ChooseTarget,
                     MaxLines = 3,
                     TierLevel = Tier.Tier1,
                     Sound = 8,
                     ScriptKey = "ao ard cradh"
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
                     NpcKey = "etaen",
                     MinLines = 0,
                     Pane = Pane.Spells,
                     TargetType = SpellTemplate.SpellUseType.ChooseTarget,
                     MaxLines = 3,
                     TierLevel = Tier.Tier1,
                     Sound = 8,
                     ScriptKey = "ao suain"
                 };

                GlobalSpellTemplateCache["ao puinsein"]
                    = new SpellTemplate()
                    {
                        Animation = 279,
                        BaseLines = 1,
                        Icon = 24,
                        LevelRate = 0.20,
                        ManaCost = 30,
                        MaxLevel = 100,
                        Name = "ao puinsein",
                        NpcKey = "etaen",
                        MinLines = 0,
                        Pane = Pane.Spells,
                        TargetType = SpellTemplate.SpellUseType.ChooseTarget,
                        MaxLines = 2,
                        TierLevel = Tier.Tier1,
                        Sound = 8,
                        ScriptKey = "ao puinsein"
                    };


                //StorageManager.SpellBucket.Save(
                //    new SpellTemplate()
                //    {
                //        Animation = 232,
                //        BaseLines = 1,
                //        Icon = 181,
                //        LevelRate = 0.20,
                //        ManaCost = 200,
                //        MaxLevel = 100,
                //        Name = "ao sith",
                //        NpcKey = "etaen",
                //        MinLines = 0,
                //        Pane = Pane.Spells,
                //        TargetType = SpellTemplate.SpellUseType.ChooseTarget,
                //        MaxLines = 2,
                //        TierLevel = Tier.Tier1,
                //        Sound = 8,
                //        ScriptKey = "ao sith",
                //        Description = "Removes all effects from a target.",
                //        Prerequisites = new LearningPredicate()
                //        {
                //            Class_Required = Class.Priest,
                //            Int_Required = 50,
                //            Wis_Required = 09,
                //            Gold_Required = 30000000,        
                //            Spell_Level_Required = 100,
                //            Spell_Required = "ao suain",
                //        },
                //    });

                GlobalSpellTemplateCache["ia naomh aite"]
                 = new SpellTemplate()
                 {
                     Animation = 383,
                     BaseLines = 4,
                     Icon = 11,
                     LevelRate = 1.50,
                     ManaCost = 500,
                     MaxLevel = 100,
                     Name = "ia naomh aite",
                     NpcKey = "etaen",
                     MinLines = 1,
                     Pane = Pane.Spells,
                     TargetType = SpellTemplate.SpellUseType.ChooseTarget,
                     MaxLines = 4,
                     TierLevel = Tier.Tier1,
                     Sound = 8,
                     ScriptKey = "aite",
                     Buff = new buff_aite()
                 };


                GlobalSpellTemplateCache["fas spiorad"] = new SpellTemplate()
                {
                    Animation = 1,
                    BaseLines = 9,
                    MinLines = 1,
                    MaxLines = 9,
                    ScriptKey = "fas spiorad",
                    Debuff = new debuff_fasspoirad(),
                    Description = "Deals 1/3 of mana in damage to the player and restores 100% mana.",
                    Icon = 26,
                    LevelRate = 0.0,
                    ManaCost = 0,
                    Pane = Pane.Spells,
                    TierLevel = Tier.Tier1,
                    Sound = 8,
                    Name = "fas spiorad",
                    MaxLevel = 100,
                    NpcKey = "dar",
                    TargetType = SpellTemplate.SpellUseType.NoTarget,
                    Prerequisites = new LearningPredicate()
                    {
                        Class_Required = Class.Wizard,
                        Con_Required = 3,
                        Int_Required = 3,
                        Wis_Required = 3,
                        Dex_Required = 3,
                        Str_Required = 3,
                        Stage_Required = ClassStage.Master,
                        ExpLevel_Required = 99
                    },
                };


                //make a 1 line staff programatically.
                //StorageManager.ItemBucket.Save(new ItemTemplate()
                //{
                //    DisplayImage = 0x8000 + 2279,
                //    Image = 151,
                //    SpellOperator = new SpellOperator(SpellOperator.SpellOperatorPolicy.Set, SpellOperator.SpellOperatorScope.all, 1, 1),
                //    Class = Class.Wizard,
                //    LevelRequired = 1,
                //    CanStack = false,
                //    Name = "Orbital Wand",
                //    Color = ItemColor.defaultgreen,
                //    CarryWeight = 5,
                //    DropRate = 0.5,
                //    Value = 10000,
                //    MaxDurability = 100000,
                //    Flags = ItemFlags.Dropable | ItemFlags.Bankable | ItemFlags.Equipable | ItemFlags.Repairable | ItemFlags.Sellable | ItemFlags.Tradeable | ItemFlags.TwoHanded | ItemFlags.Upgradeable,
                //    EquipmentSlot = ItemSlots.Weapon,
                //    ScriptName = "Weapon"
                //});

                GlobalSpellTemplateCache["dion"].Buff = new buff_dion();
                GlobalSpellTemplateCache["mor dion"].Buff = new buff_mordion();


                GlobalSpellTemplateCache["fas nadur"].NpcKey = "Dar";
                GlobalSpellTemplateCache["fas nadur"].Description = "Slightly amplifies a target's element.";
                GlobalSpellTemplateCache["fas nadur"].Prerequisites = new LearningPredicate()
                {
                    Class_Required = Class.Wizard,
                    ExpLevel_Required = 10,
                    Gold_Required = 10000,
                    Str_Required = 8,
                    Dex_Required = 8,
                    Con_Required = 8,
                    Wis_Required = 8,
                    Int_Required = 8,
                    Items_Required = new List<ItemPredicate>()
                    {
                        new ItemPredicate()
                        {
                             AmountRequired = 3,
                             Item = "Viper's Gland"
                        },
                        new ItemPredicate()
                        {
                             AmountRequired = 3,
                             Item = "Scorpions's Gland"
                        },
                        new ItemPredicate()
                        {
                             AmountRequired = 3,
                             Item = "Goblin's Skull"
                        },
                        new ItemPredicate()
                        {
                             AmountRequired = 3,
                             Item = "Wolf's Teeth"
                        },
                    },
                };
                GlobalSpellTemplateCache["mor fas nadur"].NpcKey = "Dar";
                GlobalSpellTemplateCache["mor fas nadur"].Description = "Moderately amplifies a target's element.";
                GlobalSpellTemplateCache["mor fas nadur"].Prerequisites = new LearningPredicate()
                {
                    Class_Required = Class.Wizard,
                    ExpLevel_Required = 30,
                    Gold_Required = 500000,
                    Spell_Level_Required = 100,
                    Spell_Required = "fas nadur",
                    Spell_Tier_Required = 1,
                    Str_Required = 30,
                    Wis_Required = 30,
                    Con_Required = 30,
                    Dex_Required = 30,
                    Int_Required = 30,
                    Items_Required = new List<ItemPredicate>()
                    {
                        new ItemPredicate()
                        {
                             AmountRequired = 5,
                             Item = "Goblin's Skull"
                        },
                        new ItemPredicate()
                        {
                             AmountRequired = 5,
                             Item = "Magus Diana"
                        },
                        new ItemPredicate()
                        {
                             AmountRequired = 1,
                             Item = "Hag's Replica Girdle"
                        },
                    }
                };

                #region water
                GlobalSpellTemplateCache["beag sal"].NpcKey = "Dar";
                GlobalSpellTemplateCache["beag sal"].Prerequisites = new LearningPredicate()
                {
                    Class_Required = Class.Wizard,
                    ExpLevel_Required = 3,
                    Gold_Required = 500,
                };
                GlobalSpellTemplateCache["sal"].NpcKey = "Dar";
                GlobalSpellTemplateCache["sal"].Prerequisites = new LearningPredicate()
                {
                    Class_Required = Class.Wizard,
                    ExpLevel_Required = 6,
                    Gold_Required = 3000,
                    Int_Required = 8,
                    Wis_Required = 7,
                    Spell_Required = "beag sal",
                    Spell_Level_Required = 70,
                    Spell_Tier_Required = 1,
                    Items_Required = new List<ItemPredicate>()
                    {
                        new ItemPredicate()
                        {
                             AmountRequired = 1,
                             Item = "Viper's Gland"
                        },
                        new ItemPredicate()
                        {
                             AmountRequired = 1,
                             Item = "Bee's Honey"
                        },
                    },
                };
                GlobalSpellTemplateCache["mor sal"].NpcKey = "Dar";
                GlobalSpellTemplateCache["mor sal"].Prerequisites = new LearningPredicate()
                {
                    Class_Required = Class.Wizard,
                    ExpLevel_Required = 33,
                    Gold_Required = 50000,
                    Int_Required = 28,
                    Wis_Required = 17,
                    Spell_Required = "sal",
                    Spell_Level_Required = 70,
                    Spell_Tier_Required = 1,
                    Items_Required = new List<ItemPredicate>()
                    {
                        new ItemPredicate()
                        {
                             AmountRequired = 10,
                             Item = "Viper's Gland"
                        },
                        new ItemPredicate()
                        {
                             AmountRequired = 10,
                             Item = "Bee's Honey"
                        },
                    },
                };
                GlobalSpellTemplateCache["ard sal"].NpcKey = "Dar The Forsakened";
                GlobalSpellTemplateCache["ard sal"].Prerequisites = new LearningPredicate()
                {
                    Class_Required = Class.Wizard,
                    ExpLevel_Required = 33,
                    Gold_Required = 500000,
                    Int_Required = 38,
                    Wis_Required = 87,
                    Spell_Required = "mor sal",
                    Spell_Level_Required = 100,
                    Spell_Tier_Required = 1,
                    Items_Required = new List<ItemPredicate>()
                    {
                        new ItemPredicate()
                        {
                             AmountRequired = 20,
                             Item = "Essense of Water"
                        }
                    },
                };
                #endregion

                #region Fire
                GlobalSpellTemplateCache["beag srad"].NpcKey = "Dar";
                GlobalSpellTemplateCache["beag srad"].Prerequisites = new LearningPredicate()
                {
                    Class_Required = Class.Wizard,
                    ExpLevel_Required = 3,
                    Gold_Required = 500,
                };
                GlobalSpellTemplateCache["srad"].NpcKey = "Dar";
                GlobalSpellTemplateCache["srad"].Prerequisites = new LearningPredicate()
                {
                    Class_Required = Class.Wizard,
                    ExpLevel_Required = 6,
                    Gold_Required = 3000,
                    Str_Required = 8,
                    Wis_Required = 7,
                    Spell_Required = "beag srad",
                    Spell_Level_Required = 70,
                    Spell_Tier_Required = 1,
                    Items_Required = new List<ItemPredicate>()
                    {
                        new ItemPredicate()
                        {
                             AmountRequired = 1,
                             Item = "Viper's Gland"
                        },
                        new ItemPredicate()
                        {
                             AmountRequired = 1,
                             Item = "Bee's Honey"
                        },
                    },
                };
                GlobalSpellTemplateCache["mor srad"].NpcKey = "Dar";
                GlobalSpellTemplateCache["mor srad"].Prerequisites = new LearningPredicate()
                {
                    Class_Required = Class.Wizard,
                    ExpLevel_Required = 33,
                    Gold_Required = 50000,
                    Int_Required = 28,
                    Str_Required = 17,
                    Spell_Required = "srad",
                    Spell_Level_Required = 70,
                    Spell_Tier_Required = 1,
                    Items_Required = new List<ItemPredicate>()
                    {
                        new ItemPredicate()
                        {
                             AmountRequired = 10,
                             Item = "Viper's Gland"
                        },
                        new ItemPredicate()
                        {
                             AmountRequired = 10,
                             Item = "Bee's Honey"
                        },
                    },
                };
                GlobalSpellTemplateCache["ard srad"].NpcKey = "Dar The Forsakened";
                GlobalSpellTemplateCache["ard srad"].Prerequisites = new LearningPredicate()
                {
                    Class_Required = Class.Wizard,
                    ExpLevel_Required = 65,
                    Gold_Required = 500000,
                    Str_Required = 38,
                    Wis_Required = 87,
                    Spell_Required = "mor srad",
                    Spell_Level_Required = 100,
                    Spell_Tier_Required = 1,
                    Items_Required = new List<ItemPredicate>()
                    {
                        new ItemPredicate()
                        {
                             AmountRequired = 20,
                             Item = "Essense of Fire"
                        }
                    },
                };
                #endregion

                #region Wind
                GlobalSpellTemplateCache["beag athar"].NpcKey = "Dar";
                GlobalSpellTemplateCache["beag athar"].Prerequisites = new LearningPredicate()
                {
                    Class_Required = Class.Wizard,
                    ExpLevel_Required = 3,
                    Gold_Required = 500,
                };
                #endregion

                #region Poison

                GlobalSpellTemplateCache["beag puinsein"].NpcKey = "Dar";
                GlobalSpellTemplateCache["beag puinsein"].Prerequisites = new LearningPredicate()
                {
                    Class_Required = Class.Wizard,
                    ExpLevel_Required = 8,
                    Gold_Required = 5000,
                };
                GlobalSpellTemplateCache["puinsein"].NpcKey = "Dar";
                GlobalSpellTemplateCache["puinsein"].Prerequisites = new LearningPredicate()
                {
                    Class_Required = Class.Wizard,
                    ExpLevel_Required = 15,
                    Gold_Required = 60000,
                    Spell_Tier_Required = 1,
                    Spell_Level_Required = 100,
                    Spell_Required = "beag puinsein",
                };
                GlobalSpellTemplateCache["mor puinsein"].NpcKey = "Dar";
                GlobalSpellTemplateCache["mor puinsein"].Prerequisites = new LearningPredicate()
                {
                    Class_Required = Class.Wizard,
                    ExpLevel_Required = 40,
                    Gold_Required = 150000,
                    Spell_Tier_Required = 1,
                    Spell_Level_Required = 100,
                    Spell_Required = "puinsein",
                };
                GlobalSpellTemplateCache["ard puinsein"].NpcKey = "Dar";
                GlobalSpellTemplateCache["ard puinsein"].Prerequisites = new LearningPredicate()
                {
                    Class_Required = Class.Wizard,
                    ExpLevel_Required = 85,
                    Gold_Required = 5000000,
                    Spell_Tier_Required = 1,
                    Spell_Level_Required = 100,
                    Spell_Required = "mor puinsein",
                };

                #endregion








                return false;
            });

            while (Paused)
            {
                Thread.Sleep(1000);
            }

            LoadMetaDatabase();

        }
    }
}
