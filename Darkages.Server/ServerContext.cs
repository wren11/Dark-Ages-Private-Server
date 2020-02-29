// *****************************************************************************
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


using Darkages.Network.Game;
using Darkages.Network.Login;
using Darkages.Network.Object;
using Darkages.Script.Context;
using Darkages.Storage;
using Darkages.Types;
using LiteDB;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Mono.CSharp;
using Newtonsoft.Json;
using NLog;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

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
        [field: JsonIgnore]
        private static readonly NLog.Logger Logger;

        internal static DateTime GlobalMonsterCoolDown { get; set; }

        internal static Evaluator EVALUATOR;

        public static int Errors, DefaultPort;

        public static bool Running, Paused;

        public static GameServer Game;

        public static LoginServer Lobby;

        public static ServerConstants Config;

        public static string StoragePath = @"..\..\..\LORULE_DATA";

        #region Collections
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

        public static Dictionary<string, List<Board>> GlobalBoardCache = new Dictionary<string, List<Board>>();
        #endregion

        static ServerContext()
        {
            Logger = NLog.LogManager.GetCurrentClassLogger();
        }

        [field: JsonIgnore]
        public static TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();

        [property: JsonIgnore]
        public static IPAddress IPADDR { get; } = IPAddress.Parse(File.ReadAllText("server.tbl"));

        public static string GlobalMessage { get; internal set; }

        [field: JsonIgnore]
        public static TelemetryClient AIContext = null;

        public static void Log(string message, params object[] args)
        {
            if (Logger != null)
            {
                var msg = string.Format(message, args);

                Logger.Log(LogLevel.Trace, msg);
                Report(msg);
            }
            else
                Console.WriteLine(message, args);
        }

        public static void Report(string lpMessage, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFile = "")
        {
            if (!ServerContext.Config.ErrorReporting)
                return;

            if (lpMessage.ToLower().Contains("error"))
            {
                Logger.Error(lpMessage);
                AIContext.TrackTrace(lpMessage, Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Error);
            }
            else
            {
                Logger.Trace(lpMessage);
                AIContext?.TrackTrace(lpMessage);
            }
        }

        public static void Report(Exception exception, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFile = "")
        {
            if (!ServerContext.Config.ErrorReporting)
                return;

            var msg = exception.Dump();

            if (!string.IsNullOrEmpty(msg))
            {
                Report(string.Format("[{0} -> {1}] ({2})", callerFile, callerName, msg));

                AIContext?.TrackException(exception, new Dictionary<string, string>() { { "Invoker", callerName }, { "Stack", exception.StackTrace }, { "SourceFile", callerFile } });
            }
        }

        public static void Report<T>(T obj, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFile = "")
        {
            if (!ServerContext.Config.ErrorReporting)
                return;

            var msg = obj.Dump();

            if (!string.IsNullOrEmpty(msg))
            {
                Report(string.Format("[{0} -> {1}] ({2})", callerFile, callerName, msg));
            }
        }

        public static void LoadSkillTemplates()
        {
            StorageManager.SkillBucket.CacheFromStorage();
            Logger?.Debug("Skill Templates Loaded: {0}", GlobalSkillTemplateCache.Count);
        }

        public static void LoadSpellTemplates()
        {
            StorageManager.SpellBucket.CacheFromStorage();
            Logger?.Debug("Spell Templates Loaded: {0}", GlobalSpellTemplateCache.Count);
        }

        public static void LoadItemTemplates()
        {
            StorageManager.ItemBucket.CacheFromStorage();
            Logger?.Debug("Item Templates Loaded: {0}", GlobalItemTemplateCache.Count);
        }

        public static void LoadMonsterTemplates()
        {
            StorageManager.MonsterBucket.CacheFromStorage();
            Logger?.Debug("Monster Templates Loaded: {0}", GlobalMonsterTemplateCache.Count);
        }

        public static void LoadMundaneTemplates()
        {
            StorageManager.MundaneBucket.CacheFromStorage();
            Logger?.Debug("Mundane Templates Loaded: {0}", GlobalMundaneTemplateCache.Count);
        }

        public static void LoadWarpTemplates()
        {
            StorageManager.WarpBucket.CacheFromStorage();
            Logger?.Debug("Warp Templates Loaded: {0}", GlobalWarpTemplateCache.Count);
        }

        public static void LoadWorldMapTemplates()
        {
            StorageManager.WorldMapBucket.CacheFromStorage();
            Logger?.Debug("World Map Templates Loaded: {0}", GlobalWorldMapTemplateCache.Count);
        }

        public static void LoadPopupTemplates()
        {
            StorageManager.PopupBucket.CacheFromStorage();
            Logger?.Debug("Popup Templates Loaded: {0}", GlobalPopupCache.Count);
        }

        public static void LoadMaps()
        {
            StorageManager.AreaBucket.CacheFromStorage();
            Logger?.Debug("Map Templates Loaded: {0}", GlobalMapCache.Count);
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
                    Game.Start(DefaultPort);

                    Lobby = new LoginServer(Config.ConnectionCapacity);
                    Lobby.Start(2610);
                }
                catch (SocketException e)
                {
                    Report(e);
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

        private static void SetupTelemetrics()
        {
            if (Config.ErrorReporting)
            {
                configuration                    = TelemetryConfiguration.CreateDefault();
                configuration.InstrumentationKey = ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(Config.AppInsightsKey));

                var module = new DependencyTrackingTelemetryModule();

                module.ExcludeComponentCorrelationHttpHeadersOnDomains.Add("core.windows.net");
                module.IncludeDiagnosticSourceActivities.Add("Microsoft.Azure.ServiceBus");
                module.IncludeDiagnosticSourceActivities.Add("Microsoft.Azure.EventHubs");

                configuration.TelemetryInitializers.Add(new ApplicationInsightsInitializer(configuration.InstrumentationKey));
                configuration.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());
                configuration.TelemetryInitializers.Add(new HttpDependenciesParsingTelemetryInitializer());

                module.Initialize(configuration);

                AIContext = new TelemetryClient(configuration);
            }
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
            SetupTelemetrics();

            Log("");
            Log("{0} Initializing...", Config.SERVER_TITLE);
            Log("----------------------------------------------");
            {
                try
                {
                    LoadConstants();
                    LoadAndCacheStorage();
                    InitScriptEvaluators();
                    StartServers();
                }
                catch (Exception e)
                {
                    Log("Startup Error.");
                    Report(e);
                }
            }

            Logger?.Debug("Server Online.");
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
            GlobalPopupCache = new List<PopupTemplate>();
            GlobalBoardCache = new Dictionary<string, List<Board>>();
        }

        public static void LoadConstants()
        {
            var _config_ = StorageManager.Load<ServerConstants>();

            if (_config_ == null)
            {
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
            var files = MetafileManager.GetMetafiles();

            if (files.Count > 0)
                GlobalMetaCache.AddRange(files);

            Log("Building Meta Cache: {0} loaded.", GlobalMetaCache.Count);
        }

        private static object syncLock = new object();

        public static void SaveCommunityAssets()
        {
            List<Board> tmp;

            lock (syncLock)
            {
                tmp = new List<Board>(Community);
            }

            foreach (var asset in tmp)
                asset.Save("Personal");
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

                lock (syncLock)
                {
                    Community = tmplBoards["Personal"].OrderBy(i => i.Index).ToArray();
                }

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
            {
                Logger?.Info(string.Format("Clearing Cache... Success"));
                Logger?.Info(string.Format(""));
            }

            Logger?.Trace(string.Format("Loading Server Templates..."));

            LoadMaps();
            LoadSkillTemplates();
            LoadSpellTemplates();
            LoadItemTemplates();
            LoadMonsterTemplates();
            LoadMundaneTemplates();
            LoadWarpTemplates();
            LoadPopupTemplates();
            LoadWorldMapTemplates();
            CacheCommunityAssets();
            BindTemplates();
            LoadMetaDatabase();
            LoadExtensions();

            Paused = false;

            Logger?.Trace(string.Format("Server Ready!, Listening for Connections..."));

            ServerContext.Log("Loaded {0} Boards Loaded from Cache ({1})", GlobalBoardCache.Count, GlobalBoardCache.Dump());
            ServerContext.Log("Loaded {0} Monster Templates Loaded from Cache ({1})", GlobalMonsterTemplateCache.Count, GlobalMonsterTemplateCache.Dump());
            ServerContext.Log("Loaded {0} Item Templates Loaded from Cache ({1})", GlobalItemTemplateCache.Count, GlobalItemTemplateCache.Count);
            ServerContext.Log("Loaded {0} Mundane Templates Loaded from Cache ({1})", GlobalMundaneTemplateCache, GlobalMundaneTemplateCache.Dump());
            ServerContext.Log("Loaded {0} Warp Templates Loaded from Cache ({1})", GlobalWarpTemplateCache.Count, GlobalWarpTemplateCache.Dump());
            ServerContext.Log("Loaded {0} Area Templates Loaded from Cache ({1})", GlobalMapCache.Count, GlobalMapCache.Dump());
            ServerContext.Log("Loaded {0} Popup Templates Loaded from Cache ({1})", GlobalPopupCache.Count, GlobalPopupCache.Dump());
            ServerContext.Log("Loaded {0} Buff Templates Loaded from Cache ({1})", GlobalBuffCache.Count, GlobalBuffCache.Dump());
            ServerContext.Log("Loaded {0} DeBuff Templates Loaded from Cache ({1})", GlobalDeBuffCache.Count, GlobalDeBuffCache.Dump());
            ServerContext.Log("Loaded {0} Skill Templates Loaded from Cache ({1})", GlobalSkillTemplateCache.Count, GlobalSkillTemplateCache.Dump());
            ServerContext.Log("Loaded {0} Spell Templates Loaded from Cache ({1})", GlobalSpellTemplateCache.Count, GlobalSpellTemplateCache.Dump());
            ServerContext.Log("Loaded {0} WorldMap Templates Loaded from Cache ({1})", GlobalWorldMapTemplateCache.Count, GlobalWorldMapTemplateCache.Dump());
            ServerContext.Log("Loaded {0} Reactor Templates Loaded from Cache ({1})", GlobalReactorCache.Count, GlobalReactorCache.Dump());
        }

        private static void LoadExtensions()
        {
            Logger?.Info(string.Format(""));
            Logger?.Trace(string.Format("Loading Extensions..."));

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
                    using Darkages;
                    using Darkages.Assets.locales.Scripts;
                    using Darkages.Assets.locales.Scripts.Spells;
                    using Darkages.Assets.locales.Scripts.Spells.utility;");






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