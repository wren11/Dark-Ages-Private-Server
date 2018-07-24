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
using Darkages.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

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

        public static string StoragePath = Path.GetFullPath(@"..\..\..\LORULE_DATA");

        public static string ScriptOEMPath = Path.GetFullPath(@"..\..\..\Darkages.Server\Assets\locales");

        public static Item GlobalLastItemRoll { get; set; }

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
            Console.WriteLine("[Lorule] Skill Templates Loaded: {0}", GlobalSkillTemplateCache.Count);
        }

        public static void LoadReactorTemplates()
        {
            StorageManager.ReactorBucket.CacheFromStorage();
            Console.WriteLine("[Lorule] Reactors Loaded: {0}", GlobalReactorCache.Count);
            {
                RegisterReactorCallbacks();
            }
        }

        private static void RegisterReactorCallbacks()
        {
            int registered = 0;
            foreach (var reactor in GlobalReactorCache.SelectMany(i => i.Value.Steps))
            {
                if (!string.IsNullOrEmpty(reactor.CallbackKey))
                {
                    var thisType = typeof(Darkages.ServerContext);
                    var method = thisType.GetMethod(reactor.CallbackKey);

                    reactor.Callback = (functionCallback)Delegate.CreateDelegate(typeof(functionCallback), method);
                    registered++;
                }
            }
            Console.WriteLine("[Lorule] Registered Reactor Callbacks: {0}", registered);
        }

        public static void LoadSpellTemplates()
        {
            StorageManager.SpellBucket.CacheFromStorage();
            Console.WriteLine("[Lorule] Spell Templates Loaded: {0}", GlobalSpellTemplateCache.Count);
        }

        public static void LoadItemTemplates()
        {
            StorageManager.ItemBucket.CacheFromStorage();
            Console.WriteLine("[Lorule] Item Templates Loaded: {0}", GlobalItemTemplateCache.Count);
        }

        public static void LoadMonsterTemplates()
        {
            StorageManager.MonsterBucket.CacheFromStorage();
            Console.WriteLine("[Lorule] Monster Templates Loaded: {0}", GlobalMonsterTemplateCache.Count);
        }

        public static void LoadMundaneTemplates()
        {
            StorageManager.MundaneBucket.CacheFromStorage();
            Console.WriteLine("[Lorule] Mundane Templates Loaded: {0}", GlobalMundaneTemplateCache.Count);
        }

        public static void LoadWarpTemplates()
        {
            StorageManager.WarpBucket.CacheFromStorage();
            Console.WriteLine("[Lorule] Warp Templates Loaded: {0}", GlobalWarpTemplateCache.Count);
        }

        public static void LoadWorldMapTemplates()
        {
            StorageManager.WorldMapBucket.CacheFromStorage();
            Console.WriteLine("[Lorule] World Map Templates Loaded: {0}", GlobalWorldMapTemplateCache.Count);
        }

        public static void LoadMaps()
        {
            StorageManager.AreaBucket.CacheFromStorage();
            Console.WriteLine("[Lorule] Map Templates Loaded: {0}", GlobalMapCache.Count);
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
            Console.WriteLine(Config.SERVER_TITLE);
            Console.Write(@"
┌─────────────┬─────────────────────────────────┬───────────────────────────┐
│   Author    │             website             │          Discord          │
├─────────────┼─────────────────────────────────┼───────────────────────────┤
│ Wren (Dean) │ http://darkages.creatorlink.net │ https://discord.gg/ZARPGV │
└─────────────┴─────────────────────────────────┴───────────────────────────┘

");

            {
                try
                {
                    LoadConstants();
                    Console.WriteLine("[Lorule] Loading Server Templates...");
                    LoadAndCacheStorage();
                    Console.WriteLine("[Lorule] Loading Server Templates... Completed.");
                    StartServers();
                }
                catch (Exception)
                {
                    Console.WriteLine("Startup Error.");
                }
            }
        }

        private static void EmptyCacheCollectors()
        {
            GlobalItemTemplateCache = new Dictionary<string, ItemTemplate>();
            GlobalMapCache = new Dictionary<int, Area>();
            GlobalMetaCache = new List<Metafile>();
            GlobalReactorCache = new Dictionary<string, Reactor>();
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
                Console.WriteLine("[Lorule] No config found. Generating defaults.");
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
            Console.WriteLine("[Lorule] Loading Meta Database");
            {
                var mfs = MetafileManager.GetMetafiles();

                if (mfs.Count > 0)
                {
                    GlobalMetaCache.AddRange(mfs);
                }
            }
            Console.WriteLine("[Lorule] Building Meta Cache: {0} loaded.", GlobalMetaCache.Count);
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

            try
            {
                EmptyCacheCollectors();
                lock (SyncObj)
                {
                    UpdateLocales();
                    LoadMaps();
                    LoadReactorTemplates();
                    LoadSkillTemplates();
                    LoadSpellTemplates();
                    LoadItemTemplates();
                    LoadMonsterTemplates();
                    LoadMundaneTemplates();
                    LoadWarpTemplates();
                    LoadWorldMapTemplates();
                    CacheCommunityAssets();

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
                        TierLevel = Tier.Tier1
                    };

                    GlobalSpellTemplateCache["Needle Trap"] = trap;

                    GlobalItemTemplateCache["Bible of Tricks"] = new ItemTemplate()
                    {
                        CanStack = true,
                        MaxStack = 5,
                        Color = ItemColor.defaultgreen,
                        Flags = ItemFlags.Bankable | ItemFlags.Dropable | ItemFlags.QuestRelated | ItemFlags.Sellable | ItemFlags.Tradeable,
                        Image = 0x80D8,
                        DisplayImage = 0x80D8,
                        CarryWeight = 0,
                        DropRate = 0.55,
                        LevelRequired = 3
                    };

                    GlobalSpellTemplateCache["Clone"] = new SpellTemplate()
                    {
                        Name = "Clone",
                        ScriptKey = "Clone",
                        Description = "Duplicate a Creature",
                        Animation = 246,
                        TargetAnimation = 246,
                        LevelRate = 0.12,
                        BaseLines = 2,
                        MaxLines = 9,
                        MinLines = 1,
                        DamageExponent = 0.0,
                        ElementalProperty = ElementManager.Element.None,
                        IsTrap = false,
                        ManaCost = 200,
                        NpcKey = "Old Thief",
                        Icon = 85,
                        MaxLevel = 100,
                        Pane = Pane.Spells,
                        TargetType = SpellTemplate.SpellUseType.ChooseTarget,
                        Prerequisites = new LearningPredicate()
                        {
                            Int_Required = 40,
                            Gold_Required = 50000,
                            ExpLevel_Required = 18,
                            Wis_Required = 16,
                            Class_Required = Class.Rogue,
                            Skill_Required = "Study Creature",
                            Skill_Level_Required = 80,
                            Items_Required = new List<ItemPredicate>()
                                  {
                                      new ItemPredicate()
                                      {
                                           AmountRequired = 1,
                                           Item = "Bible of Tricks"
                                      }
                                  }
                        }
                    };
                }
            }
            catch (Exception)
            {
                Paused = false;
            }

            LoadMetaDatabase();
            Paused = false;
        }

        public static void OncbResponse(Aisling sender, DialogSequence arg2)
        {

        }
    }
}
