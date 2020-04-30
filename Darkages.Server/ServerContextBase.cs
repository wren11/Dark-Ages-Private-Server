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
using Darkages.Network.Game;
using Darkages.Network.Login;
using Darkages.Network.Object;
using Darkages.Script.Context;
using Darkages.Storage;
using Darkages.Types;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Mono.CSharp;
using Newtonsoft.Json;
using NLog;
using ServiceStack;
using ServiceStack.Text;

namespace Darkages
{
    public class ServerContextBase : ObjectManager
    {
        internal static Evaluator Evaluator;
        public static int Errors;
        public static int DefaultPort;
        public static bool Running;
        public static bool Paused;
        public static GameServer Game;
        public static LoginServer Lobby;
        public static ServerConstants GlobalConfig;
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
        public static Dictionary<string, List<Board>> GlobalBoardCache = new Dictionary<string, List<Board>>();

        [field: JsonIgnore] public static TelemetryConfiguration Configuration = TelemetryConfiguration.CreateDefault();

        [field: JsonIgnore] private protected static TelemetryClient AiContext;

        [property: JsonIgnore]
        public static IPAddress IpAddress { get; } = IPAddress.Parse(File.ReadAllText("server.tbl"));

        public static string GlobalMessage { get; internal set; }


        public static void LoadSkillTemplates()
        {
            StorageManager.SkillBucket.CacheFromStorage();
            Debug("Skill Templates Loaded: {0}", GlobalSkillTemplateCache.Count);
        }

        public static void LoadSpellTemplates()
        {
            StorageManager.SpellBucket.CacheFromStorage();
            Debug("Spell Templates Loaded: {0}", GlobalSpellTemplateCache.Count);
        }

        public static void LoadItemTemplates()
        {
            StorageManager.ItemBucket.CacheFromStorage();
            Debug("Item Templates Loaded: {0}", GlobalItemTemplateCache.Count);
        }

        public static void LoadMonsterTemplates()
        {
            StorageManager.MonsterBucket.CacheFromStorage();
            Debug("Monster Templates Loaded: {0}", GlobalMonsterTemplateCache.Count);
        }

        public static void LoadMundaneTemplates()
        {
            StorageManager.MundaneBucket.CacheFromStorage();
            Debug("Mundane Templates Loaded: {0}", GlobalMundaneTemplateCache.Count);
        }

        public static void LoadWarpTemplates()
        {
            StorageManager.WarpBucket.CacheFromStorage();
            Debug("Warp Templates Loaded: {0}", GlobalWarpTemplateCache.Count);
        }

        public static void LoadWorldMapTemplates()
        {
            StorageManager.WorldMapBucket.CacheFromStorage();
            Debug("World Map Templates Loaded: {0}", GlobalWorldMapTemplateCache.Count);
        }

        public static void LoadPopupTemplates()
        {
            StorageManager.PopupBucket.CacheFromStorage();
            Debug("Popup Templates Loaded: {0}", GlobalPopupCache.Count);
        }

        public static void LoadMaps()
        {
            StorageManager.AreaBucket.CacheFromStorage();
            Debug("Map Templates Loaded: {0}", GlobalMapCache.Count);
        }

        public static void Debug(string msg, params object[] args)
        {
            var logMessage = string.Format(msg, args.Join());

            Console.WriteLine($"[Debug -> ({logMessage})");
        }

        public static void Report(string lpMessage, [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "")
        {
            if (!GlobalConfig.ErrorReporting)
                return;

            if (lpMessage.ToLower().Contains("error"))
                AiContext.TrackTrace(lpMessage, Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Error);
            else
                AiContext?.TrackTrace(lpMessage);
        }

        public static void Report(Exception exception, [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "")
        {
            if (!GlobalConfig.ErrorReporting)
                return;

            var msg = exception.Dump();

            if (!string.IsNullOrEmpty(msg))
            {
                Report($"[{Path.GetFileNameWithoutExtension(callerFile)} -> {callerName}] ({msg})");

                AiContext?.TrackException(exception,
                    new Dictionary<string, string>()
                        {{"Invoker", callerName}, {"Stack", exception.StackTrace}, {"SourceFile", callerFile}});
            }
        }

        public static void Report<T>(T obj, [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "")
        {
            if (!GlobalConfig.ErrorReporting)
                return;

            var msg = obj.Dump();

            if (!string.IsNullOrEmpty(msg))
                Report($"[{Path.GetFileNameWithoutExtension(callerFile)} -> {callerName}] ({msg})");
        }

        public static void Insight(string msg, int count, string dump, [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "")
        {
            if (!GlobalConfig.ErrorReporting)
                return;

            AiContext?.TrackTrace($"Content Loaded = ({count}) => {dump}");
        }

        private static void StartServers()
        {
#if DEBUG
            GlobalConfig.DebugMode = true;
#endif
            redo:
            {
                if (Errors > GlobalConfig.ERRORCAP)
                    Process.GetCurrentProcess().Kill();

                try
                {
                    Game = new GameServer(GlobalConfig.ConnectionCapacity);
                    Game.Start(DefaultPort);

                    Lobby = new LoginServer(GlobalConfig.ConnectionCapacity);
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

        private static void SetupTelemetrics()
        {
            if (GlobalConfig.ErrorReporting)
            {
                Configuration = TelemetryConfiguration.CreateDefault();
                Configuration.InstrumentationKey =
                    Encoding.ASCII.GetString(Convert.FromBase64String(GlobalConfig.AppInsightsKey));

                var module = new DependencyTrackingTelemetryModule();

                module.ExcludeComponentCorrelationHttpHeadersOnDomains.Add("core.windows.net");
                module.IncludeDiagnosticSourceActivities.Add("Microsoft.Azure.ServiceBus");
                module.IncludeDiagnosticSourceActivities.Add("Microsoft.Azure.EventHubs");

                Configuration.TelemetryInitializers.Add(
                    new ApplicationInsightsInitializer(Configuration.InstrumentationKey));
                Configuration.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());
                Configuration.TelemetryInitializers.Add(new HttpDependenciesParsingTelemetryInitializer());

                module.Initialize(Configuration);

                AiContext = new TelemetryClient(Configuration);
            }
        }

        protected static void DisposeGame()
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

            Debug("");
            Debug($"{GlobalConfig.SERVER_TITLE} Loading...");
            Debug("----------------------------------------------");

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
                    Debug("Startup Error.");
                    Report(e);
                }
            }
            Debug($"{GlobalConfig.SERVER_TITLE} Online and Ready!");
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
            var config = StorageManager.Load<ServerConstants>();

            if (config == null)
            {
                GlobalConfig = new ServerConstants();
                StorageManager.Save(GlobalConfig);
            }
            else
            {
                GlobalConfig = StorageManager.Load<ServerConstants>();
            }

            InitFromConfig();
        }

        public static void InitFromConfig()
        {
            DefaultPort = GlobalConfig.SERVER_PORT;

            if (!Directory.Exists(StoragePath))
                Directory.CreateDirectory(StoragePath);
        }

        public static void LoadMetaDatabase()
        {
            var files = MetafileManager.GetMetafiles();

            if (files.Any()) GlobalMetaCache.AddRange(files);
        }

        public static void SaveCommunityAssets()
        {
            List<Board> tmp;

            lock (ServerContext.syncLock)
            {
                tmp = new List<Board>(Community);
            }

            foreach (var asset in tmp)
                asset.Save("Personal");

            Debug("Saved Community Assets.");
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

                lock (ServerContext.syncLock)
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
                Debug("Clearing Cache... Success");
                Debug("");
            }

            Debug($"Loading Client Templates...");

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

            Insight("Loaded {0} Boards Loaded from Cache ({1})", GlobalBoardCache.Count, GlobalBoardCache.Dump());
            Insight("Loaded {0} Monster Templates Loaded from Cache ({1})", GlobalMonsterTemplateCache.Count,
                GlobalMonsterTemplateCache.Dump());
            Insight("Loaded {0} Item Templates Loaded from Cache ({1})", GlobalItemTemplateCache.Count,
                GlobalItemTemplateCache.Dump());
            Insight("Loaded {0} Mundane Templates Loaded from Cache ({1})", GlobalMundaneTemplateCache.Count,
                GlobalMundaneTemplateCache.Dump());
            Insight("Loaded {0} Warp Templates Loaded from Cache ({1})", GlobalWarpTemplateCache.Count,
                GlobalWarpTemplateCache.Dump());
            Insight("Loaded {0} Area Templates Loaded from Cache ({1})", GlobalMapCache.Count, GlobalMapCache.Dump());
            Insight("Loaded {0} Popup Templates Loaded from Cache ({1})", GlobalPopupCache.Count,
                GlobalPopupCache.Dump());
            Insight("Loaded {0} Buff Templates Loaded from Cache ({1})", GlobalBuffCache.Count, GlobalBuffCache.Dump());
            Insight("Loaded {0} DeBuff Templates Loaded from Cache ({1})", GlobalDeBuffCache.Count,
                GlobalDeBuffCache.Dump());
            Insight("Loaded {0} Skill Templates Loaded from Cache ({1})", GlobalSkillTemplateCache.Count,
                GlobalSkillTemplateCache.Dump());
            Insight("Loaded {0} Spell Templates Loaded from Cache ({1})", GlobalSpellTemplateCache.Count,
                GlobalSpellTemplateCache.Dump());
            Insight("Loaded {0} WorldMap Templates Loaded from Cache ({1})", GlobalWorldMapTemplateCache.Count,
                GlobalWorldMapTemplateCache.Dump());
            Insight("Loaded {0} Reactor Templates Loaded from Cache ({1})", GlobalReactorCache.Count,
                GlobalReactorCache.Dump());
        }


        private static void LoadExtensions()
        {
            Debug("");
            Debug("Loading Extensions...");

            CacheBuffs();
            Debug($"Building Buff Cache: {GlobalBuffCache.Count} Loaded.");

            CacheDebuffs();
            Debug($"Building Debuff Cache: {GlobalDeBuffCache.Count} Loaded.");
        }

        private static void InitScriptEvaluators()
        {
            Debug($"Loading Script Evaluator... {(InitScriptEvaluator() ? "Success" : "Failed")}");
        }

        public static bool InitScriptEvaluator()
        {
            var compilerContext = new CompilerContext(new CompilerSettings(), new ConsoleReportPrinter());
            var assembly = Assembly.GetExecutingAssembly();

            Evaluator = new Evaluator(compilerContext);
            Evaluator.ReferenceAssembly(assembly);
            Evaluator.InteractiveBaseClass = typeof(_Interop);

            return Evaluator.Run(@"
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
                if (GlobalDeBuffCache != null)
                    GlobalDeBuffCache[debuff.Name] = (Debuff) Activator.CreateInstance(debuff);
        }

        private static void CacheBuffs()
        {
            var listOfBuffs = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                from assemblyType in domainAssembly.GetTypes()
                where typeof(Buff).IsAssignableFrom(assemblyType)
                select assemblyType).ToArray();

            foreach (var buff in listOfBuffs)
                if (GlobalBuffCache != null)
                    GlobalBuffCache[buff.Name] = (Buff) Activator.CreateInstance(buff);
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