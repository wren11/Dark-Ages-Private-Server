﻿using Darkages.Network.Game;
using Darkages.Network.Login;
using Darkages.Storage;
using Darkages.Storage.locales.debuffs;
using Darkages.Types;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Class = Darkages.Types.Class;

namespace Darkages
{
    public class ServerContextBase
    {
        public static bool Running;

        public static bool Paused;

        public static IServerConstants Config;

        public static GameServer Game;

        public static LoginServer Lobby;

        public static string StoragePath { get; set; }

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

        [property: JsonIgnore]
        public static IPAddress IpAddress { get; } = IPAddress.Parse(File.ReadAllText("server.tbl"));

        public static string GlobalMessage { get; set; }

        public static void LoadSkillTemplates()
        {
            StorageManager.SkillBucket.CacheFromStorage();
            ServerContext.Logger($"Skill Templates Loaded: {GlobalSkillTemplateCache.Count}");
        }

        public static void LoadSpellTemplates()
        {
            StorageManager.SpellBucket.CacheFromStorage();
            ServerContext.Logger($"Spell Templates Loaded: {GlobalSpellTemplateCache.Count}");
        }

        public static void LoadItemTemplates()
        {
            StorageManager.ItemBucket.CacheFromStorage();
            ServerContext.Logger($"Item Templates Loaded: {GlobalItemTemplateCache.Count}");
        }

        public static void LoadMonsterTemplates()
        {
            StorageManager.MonsterBucket.CacheFromStorage();
            ServerContext.Logger($"Monster Templates Loaded: {GlobalMonsterTemplateCache.Count}");
        }

        public static void LoadMundaneTemplates()
        {
            StorageManager.MundaneBucket.CacheFromStorage();
            ServerContext.Logger($"Mundane Templates Loaded: {GlobalMundaneTemplateCache.Count}");
        }

        public static void LoadWarpTemplates()
        {
            StorageManager.WarpBucket.CacheFromStorage();
            ServerContext.Logger($"Warp Templates Loaded: {GlobalWarpTemplateCache.Count}");
        }

        public static void LoadWorldMapTemplates()
        {
            StorageManager.WorldMapBucket.CacheFromStorage();
            ServerContext.Logger($"World Map Templates Loaded: {GlobalWorldMapTemplateCache.Count}");
        }

        public static void LoadPopupTemplates()
        {
            StorageManager.PopupBucket.CacheFromStorage();
            ServerContext.Logger($"Popup Templates Loaded: {GlobalPopupCache.Count}");
        }

        public static void LoadMaps()
        {
            StorageManager.AreaBucket.CacheFromStorage();
            ServerContext.Logger($"Map Templates Loaded: {GlobalMapCache.Count}");
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
                ServerContext.Logger("Login server is online.");

                Lobby = new LoginServer(Config.ConnectionCapacity);
                Lobby.Start(Config.LOGIN_PORT);
                ServerContext.Logger("Game server is online.");
            }
            catch (SocketException e)
            {
                ServerContext.Error(e);
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
            ServerContext.Logger(string.Format($"{Config.SERVER_TITLE} Loading..."));

            {
                try
                {
                    LoadAndCacheStorage();
                    StartServers();
                }
                catch (Exception e)
                {
                    ServerContext.Logger(string.Format("Startup Error.", e.Message));
                }
            }
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

        public static void LoadMetaDatabase()
        {
            var files = MetafileManager.GetMetafiles();

            if (files.Any()) GlobalMetaCache.AddRange(files);
        }

        public static void SaveCommunityAssets()
        {
            List<Board> tmp;

            lock (ServerContext.SyncLock)
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

                lock (ServerContext.SyncLock)
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

           

            var blind = new SpellTemplate();
            blind.Name = "blind";
            blind.Icon = 114;
            blind.Animation = 42;
            blind.BaseLines = 4;
            blind.MaxLines  = 9;
            blind.MinLines  = 1;
            blind.Cooldown  = 3;
            blind.ManaCost  = 100;
            blind.Debuff = new debuff_blind();
            blind.ScriptKey = "blind";
            blind.NpcKey = "dar";
            blind.Pane = Pane.Spells;
            blind.LevelRate = 0.03;
            blind.TargetType = SpellTemplate.SpellUseType.ChooseTarget;
            blind.Sound = 1;
            blind.TargetAnimation = 114;
            blind.Prerequisites = new LearningPredicate();
            blind.Prerequisites.Class_Required = Class.Wizard;
            blind.Prerequisites.Con_Required = 20;
            blind.Prerequisites.Int_Required = 65;
            blind.Prerequisites.Gold_Required = 100000; 

            GlobalSpellTemplateCache.Add("blind", blind);
        }


        private static void LoadExtensions()
        {
            ServerContext.Logger("Loading Extensions...");

            CacheBuffs();
            ServerContext.Logger($"Building Buff Cache: {GlobalBuffCache.Count} Loaded.");

            CacheDebuffs();
            ServerContext.Logger($"Building Debuff Cache: {GlobalDeBuffCache.Count} Loaded.");

            ServerContext.Logger("Loading Extensions... Completed.");
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