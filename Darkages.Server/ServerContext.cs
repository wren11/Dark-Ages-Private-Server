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
using Darkages.Network.Game;
using Darkages.Network.Login;
using Darkages.Network.Object;
using Darkages.Script.Context;
using Darkages.Storage;
using Darkages.Types;
using Mono.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Class = Darkages.Types.Class;
using Console = Colorful.Console;

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

        public static string GlobalMessage { get; internal set; }

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

        public static Dictionary<string, Reactor> GlobalReactorCache =
            new Dictionary<string, Reactor>();

        public static Dictionary<string, Buff> GlobalBuffCache 
            = new Dictionary<string, Buff>();

        public static Dictionary<string, Debuff> GlobalDeBuffCache
            = new Dictionary<string, Debuff>();

        public static Board[] Community = new Board[7];

        static readonly int g = 255;
        static int r = 223;
        static int b = 250;

        public static void Log(string message, params object[] args)
        {
            Console.WriteLine(string.Format(message, args), Color.FromArgb(r % 255, g % 255, b % 255));

            r -= 18;
            b -= 9;

            if (r <= 0)
                r = 255;
            if (b <= 0)
                b = 255;
        }


        public static void LoadSkillTemplates()
        {
            StorageManager.SkillBucket.CacheFromStorage();
            Log("Skill Templates Loaded: {0}", GlobalSkillTemplateCache.Count);
        }

        public static void LoadSpellTemplates()
        {
            StorageManager.SpellBucket.CacheFromStorage();
            Log("Spell Templates Loaded: {0}", GlobalSpellTemplateCache.Count);
        }

        public static void LoadItemTemplates()
        {
            StorageManager.ItemBucket.CacheFromStorage();
            Log("Item Templates Loaded: {0}", GlobalItemTemplateCache.Count);
        }

        public static void LoadMonsterTemplates()
        {
            StorageManager.MonsterBucket.CacheFromStorage();
            Log("Monster Templates Loaded: {0}", GlobalMonsterTemplateCache.Count);
        }

        public static void LoadMundaneTemplates()
        {
            StorageManager.MundaneBucket.CacheFromStorage();
            Log("Mundane Templates Loaded: {0}", GlobalMundaneTemplateCache.Count);
        }

        public static void LoadWarpTemplates()
        {
            StorageManager.WarpBucket.CacheFromStorage();
            Log("Warp Templates Loaded: {0}", GlobalWarpTemplateCache.Count);
        }

        public static void LoadWorldMapTemplates()
        {
            StorageManager.WorldMapBucket.CacheFromStorage();
            Log("World Map Templates Loaded: {0}", GlobalWorldMapTemplateCache.Count);
        }

        public static void LoadMaps()
        {
            StorageManager.AreaBucket.CacheFromStorage();
            Log("Map Templates Loaded: {0}", GlobalMapCache.Count);
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
            Log(Config.SERVER_TITLE);
            {
                try
                {
                    LoadConstants();
                    LoadAndCacheStorage();
                    StartServers();
                    InitScriptEvaluators();
                }
                catch (Exception)
                {
                    Log("Startup Error.");
                }
            }
            Log("{0} Online.", Config.SERVER_TITLE);
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
            GlobalReactorCache = new Dictionary<string, Reactor>();
            GlobalBuffCache = new Dictionary<string, Buff>();
            GlobalDeBuffCache = new Dictionary<string, Debuff>();
        }

        public static void LoadConstants()
        {
            var _config_ = StorageManager.Load<ServerConstants>();

            if (_config_ == null)
            {
                Log("No config found. Generating defaults.");
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
            Log("Loading Meta Database");
            {
                var files = MetafileManager.GetMetafiles();

                if (files.Count > 0)
                    GlobalMetaCache.AddRange(files);
            }
            Log("Building Meta Cache: {0} loaded.", GlobalMetaCache.Count);
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

        public static void LoadAndCacheStorage()
        {
            Paused = true;

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
                CacheCommunityAssets();
                BindTemplates();
                LoadMetaDatabase();
                LoadExtensions();
            }

            var trap = new SpellTemplate()
            {
                Animation = 91,
                ScriptKey = "Needle Trap",
                IsTrap = true,
                Name = "Needle Trap",
                TargetType = SpellTemplate.SpellUseType.NoTarget,
                Icon = 16,
                ManaCost = 20,
                BaseLines = 1,
                MaxLines = 9,
                MaxLevel = 100,
                Pane = Pane.Spells,
                Sound = 89,
                MinLines = 0,
                DamageExponent = 5.0,
                Description = "Place a Small Trap damaging enemies who walk over it.",
                ElementalProperty = ElementManager.Element.Light,
                TargetAnimation = 68,
                LevelRate = 0.05,
                TierLevel = Tier.Tier1,
                Prerequisites = new LearningPredicate()
                {
                    Class_Required = Class.Rogue,
                    Dex_Required = 19,
                    Gold_Required = 1000,
                    Int_Required = 3,
                    Stage_Required = ClassStage.Class,
                    ExpLevel_Required = 5
                }
            };

            var poisontrap = new SpellTemplate()
            {
                Animation = 196,
                ScriptKey = "Poison Trap",
                IsTrap = true,
                Name = "Poison Trap",
                TargetType = SpellTemplate.SpellUseType.NoTarget,
                Icon = 35,
                ManaCost = 20,
                BaseLines = 0,
                MaxLines = 9,
                MaxLevel = 100,
                Pane = Pane.Spells,
                Sound = 89,
                MinLines = 0,
                DamageExponent = 5.0,
                Description = "Place a Small Trap that will poison enemies who walk over it.",
                ElementalProperty = ElementManager.Element.None,
                TargetAnimation = 196,
                LevelRate = 0.15,
                TierLevel = Tier.Tier1,
                Prerequisites = new LearningPredicate()
                {
                    Class_Required = Class.Rogue,
                    Dex_Required = 10,
                    Gold_Required = 500,
                    Int_Required = 8,
                    Stage_Required = ClassStage.Class,
                    ExpLevel_Required = 5
                }
            };

            GlobalSpellTemplateCache["Poison Trap"] = poisontrap;

            GlobalItemTemplateCache["Apple"] = new ItemTemplate()
            {
                LevelRequired = 1,
                Name = "Apple",
                MiniScript = "user.CurrentHp += 10;",
                CanStack = true,
                MaxStack = 100,
                Flags = ItemFlags.Consumable | ItemFlags.Bankable | ItemFlags.Stackable | ItemFlags.Sellable | ItemFlags.Tradeable | ItemFlags.QuestRelated | ItemFlags.Dropable,
                DropRate = 0.95,
                CarryWeight = 0,
                Image = 0x8028,
                DisplayImage = 0x8028,
            };

            var spider = new MonsterTemplate()
            {
                Name = "Spider",
                ScriptName = "Common Monster",
                AreaID = 426,
                Level = 1,
                BaseName = "Spider",
                AttackSpeed = 1000,
                MovementSpeed = 1000,
                CastSpeed = 10000,
                DefenseElement = ElementManager.Element.None,
                OffenseElement = ElementManager.Element.None,
                Description = "Small creature that dwells in dark places, such as crypts. Not very Aggressive, Unless Attacked.",
                ElementType = ElementQualifer.None,
                IgnoreCollision = false,
                LootType = LootQualifer.Table,
                ImageVarience = 0,
                Image = 0x4035,
                UpdateMapWide = false,
                MoodType = MoodQualifer.Neutral,
                SpawnType = SpawnQualifer.Random,
                PathQualifer = PathQualifer.Wander,
                SpawnRate = 12,
                SpawnSize = 26,
                SpawnMax = 200,
                FamilyKey = "Insect",
                Grow = false,
                Drops = new System.Collections.ObjectModel.Collection<string>()
                    {
                        "Spiders's Eye"
                    },
            };


            var item = new ItemTemplate()
            {
                Name = "Spider's Eye",
                Flags = ItemFlags.Bankable | ItemFlags.Dropable | ItemFlags.Tradeable | ItemFlags.QuestRelated | ItemFlags.Sellable | ItemFlags.Stackable,
                CanStack = true,
                MaxStack = 20,
                Value = 300,
                CarryWeight = 1,
                DropRate = 80,
                Image = 0x814F,
                LevelRequired = 1,
                Description = "These can be found by killing spiders down in the mileth crypt.",
            };

            var rawberyl = new ItemTemplate()
            {
                Name = "Raw Beryl",
                Flags = ItemFlags.Bankable | ItemFlags.Dropable | ItemFlags.Tradeable | ItemFlags.QuestRelated | ItemFlags.Sellable | ItemFlags.Stackable,
                CanStack = true,
                MaxStack = 20,
                Value = 300,
                CarryWeight = 1,
                DropRate = 40,
                Image = 0x80E8,
                DisplayImage = 0x80E8,
                LevelRequired = 1,
                Description = "A beryl that is used for upgrading.",
            };

            var rawcoral = new ItemTemplate()
            {
                Name = "Raw Coral",
                Flags = ItemFlags.Bankable | ItemFlags.Dropable | ItemFlags.Tradeable | ItemFlags.QuestRelated | ItemFlags.Sellable | ItemFlags.Stackable,
                CanStack = true,
                MaxStack = 20,
                Value = 300,
                CarryWeight = 1,
                DropRate = 20,
                Image = 0x80E9,
                DisplayImage = 0x80E9,
                LevelRequired = 1,
                Description = "A coral fragment that is used for upgrading.",
            };

            var rawruby = new ItemTemplate()
            {
                Name = "Raw Ruby",
                Flags = ItemFlags.Bankable | ItemFlags.Dropable | ItemFlags.Tradeable | ItemFlags.QuestRelated | ItemFlags.Sellable | ItemFlags.Stackable,
                CanStack = true,
                MaxStack = 20,
                Value = 300,
                CarryWeight = 1,
                DropRate = 10,
                Image = 0x80EA,
                DisplayImage = 0x80EA,
                LevelRequired = 1,
                Description = "A ruby fragment that is used for upgrading.",
            };

            var forsakenjewel = new ItemTemplate()
            {
                Name = "Forsaken Jewel",
                Flags = ItemFlags.Bankable | ItemFlags.Dropable | ItemFlags.Tradeable | ItemFlags.QuestRelated | ItemFlags.Sellable | ItemFlags.Stackable,
                CanStack = true,
                MaxStack = 20,
                Value = 300000000,
                CarryWeight = 1,
                DropRate = 1,
                Image = 0x8385,
                DisplayImage = 0x8385,
                LevelRequired = 1,
                Description = "Used to upgrade a weapon.",
            };

            GlobalItemTemplateCache["Raw Ruby"] = rawruby;
            GlobalItemTemplateCache["Raw Coral"] = rawcoral;
            GlobalItemTemplateCache["Raw Beryl"] = rawberyl;
            GlobalItemTemplateCache["Forsaken Jewel"] = forsakenjewel;


            //StorageManager.ItemBucket.Save(item);
            //StorageManager.MonsterBucket.Save(spider);

            var stab = new SkillTemplate()
            {
                Name = "Stab",
                ScriptName = "Stab",
                Cooldown = 5,
                Description = "Stab an target, dealing heavy damage. and weaken the target.",
                Icon = 5,
                FailMessage = "You failed to stab.",
                LevelRate = 0.3,
                MaxLevel = 100,
                TierLevel = Tier.Tier1,
                MissAnimation = 68,
                TargetAnimation = 70,
                Pane = Pane.Skills,
                NpcKey = "Bullop",
                PostQualifers = PostQualifer.BreakInvisible | PostQualifer.IgnoreDefense,
                Sound = 34,
                Type = SkillScope.Ability,
                Prerequisites = new LearningPredicate()
                {
                    Class_Required = Class.Rogue,
                    Dex_Required = 15,
                    ExpLevel_Required = 9,
                    Gold_Required = 15000,
                    Int_Required = 8,
                    Wis_Required = 5,
                    Str_Required = 6,
                    Items_Required = new List<ItemPredicate>()
                                {
                                    new ItemPredicate()
                                    {
                                         AmountRequired = 3,
                                         Item = "Raw Beryl",
                                    },
                                    new ItemPredicate()
                                    {
                                         Item = "Raw Ruby",
                                         AmountRequired = 1,
                                    },
                                }
                },                 
            };

            GlobalSkillTemplateCache["Stab"] = stab;

            var trap2 = new SpellTemplate()
            {
                Animation = 91,
                ScriptKey = "Stiletto Trap",
                IsTrap = true,
                Name = "Stiletto Trap",
                TargetType = SpellTemplate.SpellUseType.NoTarget,
                Icon = 52,
                ManaCost = 20,
                BaseLines = 1,
                MaxLines = 9,
                MaxLevel = 100,
                Pane = Pane.Spells,
                Sound = 60,
                MinLines = 0,
                DamageExponent = 5.0,
                Description = "Place a medium Trap damaging enemies who walk over it.",
                ElementalProperty = ElementManager.Element.Light,
                TargetAnimation = 394,
                LevelRate = 0.05,
                TierLevel = Tier.Tier1,
                Prerequisites = new LearningPredicate()
                {
                    Class_Required = Class.Rogue,
                    Dex_Required = 6,
                    Gold_Required = 5000,
                    Int_Required = 8,
                    Stage_Required = ClassStage.Class,
                    ExpLevel_Required = 9,
                    Items_Required = new List<ItemPredicate>()
                        {
                            new ItemPredicate()
                            {
                                 Item = "Snow Secret",
                                 AmountRequired = 1,
                            }
                        }
                }
            };

            GlobalSpellTemplateCache["Needle Trap"] = trap;
            GlobalSpellTemplateCache["Stiletto Trap"] = trap2;




            GlobalReactorCache["tut_reactor_1"] = new Reactor()
            {
                CallerType = ReactorQualifer.Map,
                Description = "test description",
                Name = "tut_reactor_1",
                Location = new Position(40, 21),
                MapId = 101,
                ScriptKey = "example reactor",
                QuestReward = new Quest()
                {
                    ExpRewards = new List<uint>() { 500 },
                    GoldReward = 500,
                    LegendRewards = new List<Legend.LegendItem>()
                         {
                             new Legend.LegendItem()
                             {
                                  Category = Convert.ToString(LegendIcon.Wizard),
                                  Color = (byte)LegendColor.White,
                                  Icon = (byte)LegendIcon.Heart,
                                  Value = "Witnessed the Magical Tree.",
                             }
                         },
                },

                Steps = new List<DialogSequence>()
                      {
                          new DialogSequence()
                          {
                               Title = "Hi",
                               DisplayText = "This tree does not belong here. you can see it's out of place. It was stolen.",
                               DisplayImage = 0x4004,
                               CanMoveBack = false,
                               CanMoveNext = true,
                          },
                          new DialogSequence()
                          {
                               Title = "Hi",
                               DisplayText = "I found it here, We've all been looking for it. I must go report this. If i don't make it. Please visit East Woodlands and travel north.",
                               DisplayImage = 0x4004,
                               CanMoveBack = false,
                               CanMoveNext = true,
                          },
                          new DialogSequence()
                          {
                               Title = "Hi",
                               DisplayText = "Oh yeah, Take this. \n\nRemember!, If i don't make it back - Visit my family in East Woodlands.",
                               DisplayImage = 0x4004,
                               CanMoveBack = false,
                               CanMoveNext = true,
                          },
                      }
            };


            Paused = false;
        }

        private static void LoadExtensions()
        {
            CacheBuffs();
            Log("Building Buff Cache: {0} loaded.", GlobalBuffCache.Count);
            CacheDebuffs();
            Log("Building Debuff Cache: {0} loaded.", GlobalDeBuffCache.Count);
        }

        private static void InitScriptEvaluators()
        {
            Log("Loading Script Evaluator...");

            try
            {
                InitScriptEvaluator();
                Log("Loading Script Evaluator... Success");
            }
            catch
            {
                Log("Loading Script Evaluator... Error.");
            }
        }

        public static void InitScriptEvaluator()
        {
            Evaluator.Init(new string[0]);
            var assembly = Assembly.Load("Darkages.Server");
            Evaluator.ReferenceAssembly(assembly);

            @"  using Darkages.Common;
                using Darkages.Common;
                using Darkages.Network.Game;
                using Darkages.Network.Login;
                using Darkages.Network.Object;
                using Darkages.Script.Context;
                using Darkages.Storage;
                using Darkages.Types;
                using System;
                using System.Collections.Generic;
                using System.Diagnostics;
                using System.IO;
                using System.Linq;
                using System.Net;
                using System.Reflection;
                using System.Threading.Tasks;".Run();

            Context.Items["game"] = Game;
        }

        private static void CacheDebuffs()
        {
            var listOfDebuffs = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                                 from assemblyType in domainAssembly.GetTypes()
                                 where typeof(Debuff).IsAssignableFrom(assemblyType)
                                 select assemblyType).ToArray();

            foreach (var debuff in listOfDebuffs)
            {
                GlobalDeBuffCache[debuff.Name] = (Debuff)Activator.CreateInstance(debuff);
            }
        }

        private static void CacheBuffs()
        {
            var listOfBuffs = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                               from assemblyType in domainAssembly.GetTypes()
                               where typeof(Buff).IsAssignableFrom(assemblyType)
                               select assemblyType).ToArray();

            foreach (var buff in listOfBuffs)
            {
                GlobalBuffCache[buff.Name] = (Buff)Activator.CreateInstance(buff);
            }
        }

        private static void BindTemplates()
        {
            foreach (var spell in ServerContext.GlobalSpellTemplateCache.Values)
                spell.Prerequisites?.AssociatedWith(spell);
            foreach (var skill in ServerContext.GlobalSkillTemplateCache.Values)
                skill.Prerequisites?.AssociatedWith(skill);
        }
    }
}
