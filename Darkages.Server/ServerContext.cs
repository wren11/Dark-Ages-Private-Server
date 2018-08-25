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

        public static Dictionary<string, Reactor> GlobalReactorCache =
            new Dictionary<string, Reactor>();

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
            GlobalReactorCache = new Dictionary<string, Reactor>();
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
                var files = MetafileManager.GetMetafiles();

                if (files.Count > 0)
                    GlobalMetaCache.AddRange(files);
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
                    CacheCommunityAssets();
                    BindTemplates();
                    LoadMetaDatabase();
                }


                var trap = new SpellTemplate()
                {
                    Animation = 91,
                    ScriptKey = "Needle Trap",
                    IsTrap = true,
                    Name = "Needle Trap",
                    TargetType = SpellTemplate.SpellUseType.NoTarget,
                    Icon = 20,
                    ManaCost = 20,
                    BaseLines = 2,
                    MaxLines = 9,
                    MaxLevel = 100,
                    Pane = Pane.Spells,
                    Sound = 0,
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
                        Dex_Required = 6,
                        Gold_Required = 1000,
                        Int_Required = 8,
                        Stage_Required = ClassStage.Class,
                        ExpLevel_Required = 5
                    }
                };



                GlobalSpellTemplateCache["Needle Trap"] = trap;





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

                return false;
            });
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
