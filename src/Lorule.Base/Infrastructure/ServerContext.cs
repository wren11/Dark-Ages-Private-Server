#region

using Darkages.Network.Game;
using Darkages.Network.Login;
using Darkages.Storage;
using Darkages.Systems.CLI;
using Darkages.Types;
using Darkages.Types.Templates;
using Newtonsoft.Json;
using Pyratron.Frameworks.Commands.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

#endregion

namespace Darkages
{
    public class ServerContext : IServerContext
    {
        public static CommandParser Parser { get; set; }

        public static bool Running;

        public static bool Paused;

        public static IServerConstants Config;

        public static GameServer Game;

        public static LoginServer Lobby;

        public static List<Metafile> GlobalMetaCache = new List<Metafile>();

        public static Dictionary<int, Area> GlobalMapCache =
            new Dictionary<int, Area>(new Dictionary<int, Area>());

        public static List<MonsterTemplate> GlobalMonsterTemplateCache =
            new List<MonsterTemplate>();

        public static Dictionary<string, SkillTemplate> GlobalSkillTemplateCache =
            new Dictionary<string, SkillTemplate>();

        public static Dictionary<string, SpellTemplate> GlobalSpellTemplateCache =
            new Dictionary<string, SpellTemplate>();

        public static Dictionary<string, ItemTemplate> GlobalItemTemplateCache =
            new Dictionary<string, ItemTemplate>(new Dictionary<string, ItemTemplate>());

        public static Dictionary<string, NationTemplate> GlobalNationTemplateCache =
            new Dictionary<string, NationTemplate>();

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

        public static Dictionary<int, Party> GlobalGroupCache
            = new Dictionary<int, Party>();

        public static Dictionary<string, ServerTemplate> GlobalServerVarCache
            = new Dictionary<string, ServerTemplate>();

        public static ICollection<string> Redirects = new List<string>();

        public static Board[] Community = new Board[7];

        public static Dictionary<string, List<Board>> GlobalBoardCache = new Dictionary<string, List<Board>>();

        public static string StoragePath { get; set; }

        [property: JsonIgnore]
        public static IPAddress IpAddress { get; } = IPAddress.Parse(File.ReadAllText("server.tbl"));

        public static string GlobalMessage { get; set; }

        public static void LoadNationsTemplates()
        {
            StorageManager.NationBucket.CacheFromStorage();
            Console.WriteLine($"Nation Templates Loaded: {GlobalNationTemplateCache.Count}");
        }

        public static void LoadSkillTemplates()
        {
            StorageManager.SkillBucket.CacheFromStorage();
            Console.WriteLine($"Skill Templates Loaded: {GlobalSkillTemplateCache.Count}");
        }

        public static void LoadSpellTemplates()
        {
            StorageManager.SpellBucket.CacheFromStorage();
            Console.WriteLine($"Spell Templates Loaded: {GlobalSpellTemplateCache.Count}");
        }

        public static void LoadItemTemplates()
        {
            StorageManager.ItemBucket.CacheFromStorage();
            Console.WriteLine($"Item Templates Loaded: {GlobalItemTemplateCache.Count}");
        }

        public static void LoadMonsterTemplates()
        {
            StorageManager.MonsterBucket.CacheFromStorage();
            Console.WriteLine($"Monster Templates Loaded: {GlobalMonsterTemplateCache.Count}");
        }

        public static void LoadMundaneTemplates()
        {
            StorageManager.MundaneBucket.CacheFromStorage();
            Console.WriteLine($"Mundane Templates Loaded: {GlobalMundaneTemplateCache.Count}");
        }

        public static void LoadWarpTemplates()
        {
            StorageManager.WarpBucket.CacheFromStorage();
            Console.WriteLine($"Warp Templates Loaded: {GlobalWarpTemplateCache.Count}");
        }

        public static void LoadServerTemplates()
        {
            StorageManager.ServerArgBucket.CacheFromStorage();
            Console.WriteLine($"Server Templates Loaded: {GlobalServerVarCache.Count}");
        }

        public static void LoadWorldMapTemplates()
        {
            StorageManager.WorldMapBucket.CacheFromStorage();
            Console.WriteLine($"World Map Templates Loaded: {GlobalWorldMapTemplateCache.Count}");
        }

        public static void LoadPopupTemplates()
        {
            StorageManager.PopupBucket.CacheFromStorage();
            Console.WriteLine($"Popup Templates Loaded: {GlobalPopupCache.Count}");
        }

        public static void LoadMaps()
        {
            StorageManager.AreaBucket.CacheFromStorage();
            Console.WriteLine($"Map Templates Loaded: {GlobalMapCache.Count}");
        }

        private static void StartServers()
        {
#if DEBUG
            Config.DebugMode = true;
#endif
            try
            {
                Game = new GameServer(Config.ConnectionCapacity);
                Game.Start(Config.SERVER_PORT);
                Lobby = new LoginServer(Config.ConnectionCapacity);
                Lobby.Start(Config.LOGIN_PORT);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Login server is online.");
                Console.WriteLine("Game server is online.");
            }
            catch (SocketException e)
            {
                Error(e);
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
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{Config.SERVER_TITLE} - Powered By Lorule. https://github.com/wren11/DarkAges-Lorule-Server");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("");
            
            try
            {
                LoadAndCacheStorage();
                StartServers();
            }
            catch (Exception)
            {
                Console.WriteLine("Startup Error.");
            }
        }

        private static void EmptyCacheCollectors()
        {
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

        public static void LoadMetaDatabase()
        {
            var files = MetafileManager.GetMetaFiles();

            if (files.Any()) GlobalMetaCache.AddRange(files);
        }

        public static void SaveCommunityAssets()
        {
            List<Board> tmp;

            lock (SyncLock)
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
                var dirs = Directory.GetDirectories(Path.Combine(StoragePath, "community\\boards"));
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

                lock (SyncLock)
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
            LoadMaps();
            LoadServerTemplates();
            LoadNationsTemplates();
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
        }

        private static void LoadExtensions()
        {
            Console.WriteLine("Loading Extensions...");

            CacheBuffs();
            Console.WriteLine($"Building Buff Cache: {GlobalBuffCache.Count} Loaded.");

            CacheDebuffs();
            Console.WriteLine($"Building Debuff Cache: {GlobalDeBuffCache.Count} Loaded.");

            Console.WriteLine("Loading Extensions... Completed.");
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

            foreach (var spell in GlobalSpellTemplateCache.Values)
            {
                if (spell?.LearningRequirements == null) continue;
                foreach (var req in spell.LearningRequirements)
                    req.AssociatedWith(spell);
            }

            foreach (var skill in GlobalSkillTemplateCache.Values)
            {
                if (skill?.LearningRequirements == null) continue;
                foreach (var req in skill.LearningRequirements)
                    req.AssociatedWith(skill);
            }
        }

        public static object SyncLock = new object();
        public static Action<Exception> Error { get; set; }
        public static Action<string> Logger { get; set; }

        public void InitFromConfig(string storagePath)
        {
            StoragePath = storagePath;

            if (!Directory.Exists(StoragePath))
                Directory.CreateDirectory(StoragePath);
        }

        public virtual void Shutdown()
        {
            DisposeGame();
        }

        public virtual void Start(IServerConstants config, Action<string> log, Action<Exception> error)
        {
            Error = error ?? throw new ArgumentNullException(nameof(error));
            Logger = log ?? throw new ArgumentNullException(nameof(log));
            Config = config ?? throw new ArgumentNullException(nameof(config));

            Commander.CompileCommands();



            Startup();
            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.WriteLine(Environment.NewLine);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("GM Commands");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("-------------------------------------------------------------------");

            Console.ForegroundColor = ConsoleColor.Gray;
            foreach (var command in Parser.Commands)
            {
                Console.WriteLine(command.ShowHelp());
            }

            CommandHandler();
        }

        private static void CommandHandler()
        {
            //TODO.
        }
    }
}