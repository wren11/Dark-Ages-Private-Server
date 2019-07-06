﻿// *****************************************************************************
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
// *************************************************************************


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Darkages.Interops;
using Darkages.Network.Game;
using Darkages.Network.Login;
using Darkages.Network.Object;
using Darkages.Script.Context;
using Darkages.Storage;
using Darkages.Types;
using Mono.CSharp;
using Newtonsoft.Json;

namespace Darkages
{
    /// <summary>
    ///     The Main Application Context Used to Couple All Information used to Manage Running Servers and Clients and
    ///     Storage.
    /// </summary>
    /// <remarks>Implements the ObjectManager Class</remarks>
    /// <seealso cref="Darkages.Network.Object.ObjectManager" />
    public class ServerContext : ObjectManager
    {
        internal static object SyncObj = new object();

        internal static Evaluator EVALUATOR;

        public static int Errors, DefaultPort;

        public static bool Running, Paused;

        public static GameServer Game;

        public static LoginServer Lobby;

        public static ServerConstants Config;

        public static string StoragePath = @"..\..\..\LORULE_DATA";

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

        public static List<PopupTemplate> GlobalPopupCache
            = new List<PopupTemplate>();

        public static Board[] Community = new Board[7];

        public static Dictionary<string, List<Board>> GlobalBoardCache
            = new Dictionary<string, List<Board>>();

        static ServerContext()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        public static IPAddress Ipaddress => IPAddress.Parse(File.ReadAllText("server.tbl"));

        public static string GlobalMessage { get; internal set; }

        [field: JsonIgnore] public static ServerInformation ILog { get; set; } = new ServerInformation();

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var error = (Exception) e.ExceptionObject;
            ILog?.Error("Unhandled Exception", error);
        }

        public static void Log(string message, params object[] args)
        {
            if (ILog != null)
                ILog.Info(message, args);
            else
                Console.WriteLine(message, args);
        }

        public static void LoadSkillTemplates()
        {
            StorageManager.SkillBucket.CacheFromStorage();
            ILog?.Debug("Skill Templates Loaded: {0}", GlobalSkillTemplateCache.Count);
        }

        public static void LoadSpellTemplates()
        {
            StorageManager.SpellBucket.CacheFromStorage();
            ILog?.Debug("Spell Templates Loaded: {0}", GlobalSpellTemplateCache.Count);
        }

        public static void LoadItemTemplates()
        {
            StorageManager.ItemBucket.CacheFromStorage();
            ILog?.Debug("Item Templates Loaded: {0}", GlobalItemTemplateCache.Count);
        }

        public static void LoadMonsterTemplates()
        {
            StorageManager.MonsterBucket.CacheFromStorage();
            ILog?.Debug("Monster Templates Loaded: {0}", GlobalMonsterTemplateCache.Count);
        }

        public static void LoadMundaneTemplates()
        {
            StorageManager.MundaneBucket.CacheFromStorage();
            ILog?.Debug("Mundane Templates Loaded: {0}", GlobalMundaneTemplateCache.Count);
        }

        public static void LoadWarpTemplates()
        {
            StorageManager.WarpBucket.CacheFromStorage();
            ILog?.Debug("Warp Templates Loaded: {0}", GlobalWarpTemplateCache.Count);
        }

        public static void LoadWorldMapTemplates()
        {
            StorageManager.WorldMapBucket.CacheFromStorage();
            ILog?.Debug("World Map Templates Loaded: {0}", GlobalWorldMapTemplateCache.Count);
        }

        public static void LoadMaps()
        {
            StorageManager.AreaBucket.CacheFromStorage();
            ILog?.Debug("Map Templates Loaded: {0}", GlobalMapCache.Count);
        }

        private static void StartServers()
        {
#if DEBUG
            Config.DebugMode = true;
#endif
            redo:
            {
                if (Errors > Config.ERRORCAP)
                    Process.GetCurrentProcess().Kill();

                try
                {
                    Game = new GameServer(Config.ConnectionCapacity);
                    Game.StartAsync(DefaultPort);

                    Lobby = new LoginServer(Config.ConnectionCapacity);
                    Lobby.StartAsync(2610);
                }
                catch (SocketException)
                {
                    {
                        ++DefaultPort;
                        Errors++;
                    }
                    goto redo;
                }
            }
        }

        public virtual void Start()
        {
            Startup();
        }

        public virtual void Shutdown()
        {
            DisposeGame();
        }

        public static void Terminate()
        {
            DisposeGame();
        }

        private static void DisposeGame()
        {
            Game?.Abort();
            Lobby?.Abort();

            Game = null;
            Lobby = null;
            Running = false;
        }

        public static void Startup()
        {
            Log(Config.SERVER_TITLE + " Server: Starting up...");
            Log("----------------------------------------------");
            {
                try
                {
                    LoadConstants();
                    LoadAndCacheStorage();
                    InitScriptEvaluators();
                    StartServers();
                }
                catch (Exception)
                {
                    Log("Startup Error.");
                }
            }
            Log("----------------------------------------------");
            Log("{0} World Server: Online.", Config.SERVER_TITLE);
        }


        private static void EmptyCacheCollectors()
        {
            GlobalItemTemplateCache = new Dictionary<string, ItemTemplate>();
            GlobalMapCache = new Dictionary<int, Area>();
            GlobalMetaCache = new List<Metafile>();
            GlobalMonsterTemplateCache = new List<MonsterTemplate>();
            GlobalMundaneTemplateCache = new Dictionary<string, MundaneTemplate>();
            GlobalSkillTemplateCache = new Dictionary<string, SkillTemplate>();
            GlobalSpellTemplateCache = new Dictionary<string, SpellTemplate>();
            GlobalWarpTemplateCache = new List<WarpTemplate>();
            GlobalWorldMapTemplateCache = new Dictionary<int, WorldMapTemplate>();
            GlobalReactorCache = new Dictionary<string, Reactor>();
            GlobalBuffCache = new Dictionary<string, Buff>();
            GlobalDeBuffCache = new Dictionary<string, Debuff>();
            GlobalBoardCache = new Dictionary<string, List<Board>>();
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

            foreach (var asset in tmp) asset.Save("Personal");
        }

        public static void CacheCommunityAssets()
        {
            if (Community != null)
            {
                var dirs = Directory.GetDirectories(Path.Combine(StoragePath, "Community\\Boards"));
                var tmplBoards = new Dictionary<string, List<Board>>();

                foreach (var dir in dirs.Select(i => new DirectoryInfo(i)))
                {
                    var boards = Board.CacheFromStorage(dir.FullName);

                    if (boards == null) continue;

                    if (dir.Name == "Personal")
                        if (boards.Find(i => i.Index == 0) == null)
                            boards.Add(new Board("Mail", 0, true));

                    if (!tmplBoards.ContainsKey(dir.Name)) tmplBoards[dir.Name] = new List<Board>();

                    tmplBoards[dir.Name].AddRange(boards);
                }

                Community = tmplBoards["Personal"].OrderBy(i => i.Index).ToArray();

                foreach (var obj in tmplBoards)
                {
                    if (!GlobalBoardCache.ContainsKey(obj.Key)) GlobalBoardCache[obj.Key] = new List<Board>();

                    GlobalBoardCache[obj.Key].AddRange(obj.Value);
                }
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

            var template = new UserClickPopup
            {
                Description = "A user Click Popup",
                Ephemeral = true,
                SpriteId = 0x4005,
                Timeout = 200,
                Name = "Woodlands Guard",
                X = 31,
                Y = 23,
                MapId = 200,
                YamlKey = "Woodlands Guard"
            };

            GlobalPopupCache.Add(template);

            var user_drop = new ItemDropPopup
            {
                Description = "Polish a gem and hopefully improve it's quality.",
                Ephemeral = true,
                ItemName = "Raw Beryl",
                Name = "Polish Gem",
                Group = "Popups",
                SpriteId = 33000,
                Timeout = 15,
                YamlKey = "gem_polishing"
            };

            var user_pickup_drop = new ItemPickupPopup
            {
                Description = "Drop [ItemName] onto the reactor [gem_polishing] invoke the gem_polishing yaml script.",
                Ephemeral = true,
                ItemName = "Raw Beryl",
                Name = "Polish Gem",
                Group = "Popups",
                SpriteId = 33000,
                Timeout = 15,
                YamlKey = "gem_polishing"
            };


            var user_walk = new UserWalkPopup
            {
                Description = "Call yaml script when user walks to X, Y on MapId.",
                MapId = 301,
                X = 10,
                Y = 8,
                SpriteId = 0x4032,
                Group = "Popups",
                Name = "Crazy Quest",
                YamlKey = "gem_polishing"
            };

            //GlobalPopupCache.Add(user_drop);
            GlobalPopupCache.Add(user_pickup_drop);
            GlobalPopupCache.Add(user_walk);
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
            Log("Loading Script Evaluator... {0}", InitScriptEvaluator() ? "Success" : "Failed");
        }

        public static bool InitScriptEvaluator()
        {
            var compilerContext = new CompilerContext(new CompilerSettings(), new ConsoleReportPrinter());
            var assembly = Assembly.GetExecutingAssembly();

            EVALUATOR = new Evaluator(compilerContext);
            EVALUATOR.ReferenceAssembly(assembly);
            EVALUATOR.InteractiveBaseClass = typeof(_Interop);

            return EVALUATOR.Run(@"
                    using Darkages.Common;
                    using Darkages.Interops;
                    using Darkages.Network.Game;
                    using Darkages.Network.Object;
                    using Darkages.Script.Context;
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
                    using System.Reflection;
                    using Darkages;");
        }

        private static void CacheDebuffs()
        {
            var listOfDebuffs = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                from assemblyType in domainAssembly.GetTypes()
                where typeof(Debuff).IsAssignableFrom(assemblyType)
                select assemblyType).ToArray();

            foreach (var debuff in listOfDebuffs)
                GlobalDeBuffCache[debuff.Name] = (Debuff) Activator.CreateInstance(debuff);
        }

        private static void CacheBuffs()
        {
            var listOfBuffs = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                from assemblyType in domainAssembly.GetTypes()
                where typeof(Buff).IsAssignableFrom(assemblyType)
                select assemblyType).ToArray();

            foreach (var buff in listOfBuffs) GlobalBuffCache[buff.Name] = (Buff) Activator.CreateInstance(buff);
        }

        private static void BindTemplates()
        {
            foreach (var spell in GlobalSpellTemplateCache.Values)
                spell.Prerequisites?.AssociatedWith(spell);
            foreach (var skill in GlobalSkillTemplateCache.Values)
                skill.Prerequisites?.AssociatedWith(skill);
        }
    }
}