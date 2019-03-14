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
using Darkages.Interops;
using Darkages.Network.Game;
using Darkages.Network.Object;
using Darkages.Script.Context;
using Darkages.Storage;
using Darkages.Storage.locales.Buffs;
using Darkages.Types;
using Mono.CSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Class = Darkages.Types.Class;

namespace Darkages
{
    public class ServerContext : ObjectManager
    {
        internal static object SyncObj = new object();
        internal static Exception UnhandledException = new Exception();

        public static int Errors, DefaultPort;

        public static bool Running, Paused;

        public static GameServer Game;

        public static ServerConstants Config;

        public static IPAddress Ipaddress => IPAddress.Parse(File.ReadAllText("server.tbl"));

        public static string GlobalMessage { get; internal set; }

        public static string StoragePath = @"..\..\..\LORULE_DATA";

        [JsonIgnore]
        private static ServerInformation _info = new ServerInformation();

        public static ServerInformation Info
        {
            get { return _info; }
            set { _info = value; }
        }

        static ServerContext()
        {
            //BufferPool.SetBufferManagerBufferPool(1000, 0x10000);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception error = (Exception)e.ExceptionObject;
            Info?.Error("Unhandled Exception", error);
        }

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

        public static Dictionary<string, Aisling> ConnectedBots
            = new Dictionary<string, Aisling>();

        public static Board[] Community = new Board[7];

        public static void Log(string message, params object[] args)
        {
            if (Info != null)
            {
                Info.Info(message, args);
            }
            else
            {
                Console.WriteLine(message, args);
            }
        }


        public static void LoadSkillTemplates()
        {
            StorageManager.SkillBucket.CacheFromStorage();
            Info?.Debug("Skill Templates Loaded: {0}", GlobalSkillTemplateCache.Count);
        }

        public static void LoadSpellTemplates()
        {
            StorageManager.SpellBucket.CacheFromStorage();
            Info?.Debug("Spell Templates Loaded: {0}", GlobalSpellTemplateCache.Count);
        }

        public static void LoadItemTemplates()
        {
            StorageManager.ItemBucket.CacheFromStorage();
            Info?.Debug("Item Templates Loaded: {0}", GlobalItemTemplateCache.Count);
        }

        public static void LoadMonsterTemplates()
        {
            StorageManager.MonsterBucket.CacheFromStorage();
            Info?.Debug("Monster Templates Loaded: {0}", GlobalMonsterTemplateCache.Count);
        }

        public static void LoadMundaneTemplates()
        {
            StorageManager.MundaneBucket.CacheFromStorage();
            Info?.Debug("Mundane Templates Loaded: {0}", GlobalMundaneTemplateCache.Count);
        }

        public static void LoadWarpTemplates()
        {
            StorageManager.WarpBucket.CacheFromStorage();
            Info?.Debug("Warp Templates Loaded: {0}", GlobalWarpTemplateCache.Count);
        }

        public static void LoadWorldMapTemplates()
        {
            StorageManager.WorldMapBucket.CacheFromStorage();
            Info?.Debug("World Map Templates Loaded: {0}", GlobalWorldMapTemplateCache.Count);
        }

        public static void LoadMaps()
        {
            StorageManager.AreaBucket.CacheFromStorage();
            Info?.Debug("Map Templates Loaded: {0}", GlobalMapCache.Count);
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
                Game = null;
            }
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
                var boards = Board.CacheFromStorage();

                if (boards.Find(i => i.Index == 0) == null)
                {
                    boards.Add(new Board("Mail", 0, true));
                }

                Community = boards.OrderBy(i => i.Index).ToArray();
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


            Paused = false;




            //How to make an "Earth Bodice" Monk level 1 Female Armor.
            //--------------------------------------------------------------------------------------------------------------

            var armor_template = new ItemTemplate();

            armor_template.Name = "Earth's Bodice";

            //armor_template.Image, you can find this number here: http://www.vorlof.com/general/searcharmors.html 
            //on vorlof's site, You want to use the "Appearance ID" here.
            //on the site it says that female monk level 1 armor has appearance id 3, so i will use this as my image id.
            armor_template.Image = 3;

            //armor_template.DisplayImage, This is the sprite id, It describes how the image looks on the ground, in the inventory, profile display ect.
            //This starts at 32768 + The ID on column "On a Female" or "On a Male" on vorlofs site.
            //For example, On vorlof's site it says trhe female male monk armor is 117
            //so i would do 32768 + 117, resulting in 32885, so my DisplayImage would be 32885
            armor_template.DisplayImage = 32885;



            //set some item flags. self explainatory.
            armor_template.Flags =  ItemFlags.Equipable  | ItemFlags.Repairable  | ItemFlags.Bankable | ItemFlags.Dropable | ItemFlags.Perishable;


            //who can wear it? Male|Female|Both, the Earth Bodice is for Females, so apply that.
            armor_template.Gender = Gender.Female;

            //item attributes

            //ac mod + 7
            armor_template.AcModifer = new StatusOperator(StatusOperator.Operator.Add, 7);
            
            //you can use other mods to set other attributes, see below:
            //armor_template.AcModifer
            //armor_template.StrModifer
            //armor_template.IntModifer
            //armor_template.WisModifer
            //armor_template.ConModifer
            //armor_template.DexModifer
            //armor_template.HealthModifer
            //armor_template.ManaModifer
            //armor_template.MrModifer
            //armor_template.HitModifer
            //armor_template.DmgModifer
            
            //level to equip
            armor_template.LevelRequired = 1;

            //value of the item, resell value ect.
            armor_template.Value = 850;

            //the item's max durability
            armor_template.MaxDurability = 3000;

            //how much weight will i need to carry it?
            armor_template.CarryWeight = 4;

            //monk armor, set the correct class.
            armor_template.Class = Class.Monk;

            //on equipped, where will it go?
            armor_template.EquipmentSlot = ItemSlots.Armor;

            //we are going to use the Armor Script to handle the rest.
            armor_template.ScriptName = "Armor";

            //what is the stage required? 
            armor_template.StageRequired = ClassStage.Class;


            //Drop rate related stuff:

            //0.20 means that is has 0.20% to be added to the loot table.
            armor_template.DropRate = 0.20;

            //if on the loot table. what is the weighted chance that it will become an item of rarity?
            armor_template.Weight = 40;


            //this is all we need for an Earth Bodice.
            //lets save it to the template database.
            StorageManager.ItemBucket.SaveOrReplace(armor_template);

            GlobalItemTemplateCache["Earth's Bodice"] = armor_template;

            //SyncStorage();
        }

        private static void SyncStorage()
        {
            foreach (var obj in GlobalItemTemplateCache.Select(i => i.Value))
                StorageManager.ItemBucket.SaveOrReplace(obj);

            foreach (var obj in GlobalSpellTemplateCache.Select(i => i.Value))
                StorageManager.SpellBucket.SaveOrReplace(obj);

            foreach (var obj in GlobalSkillTemplateCache.Select(i => i.Value))
                StorageManager.SkillBucket.SaveOrReplace(obj);

            foreach (var obj in GlobalMonsterTemplateCache)
                StorageManager.MonsterBucket.SaveOrReplace(obj);

            foreach (var obj in GlobalMundaneTemplateCache.Select(i => i.Value))
                StorageManager.MundaneBucket.SaveOrReplace(obj);
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
            try
            {
                Evaluator.Init(new[] { "verbose=1;" });
                Evaluator.GetVars();

                var assembly = Assembly.Load("Darkages.Server");

                Evaluator.ReferenceAssembly(assembly);


                @"  using Darkages.Common;
                using Darkages.Network.Game;
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
            catch
            {
            }
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
