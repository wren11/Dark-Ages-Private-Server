using Darkages.Network.Game;
using Darkages.Storage;
using Darkages.Types;
using Pyratron.Frameworks.Commands.Parser;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Darkages.Systems.CLI
{
    public class Commander
    {
        static Commander()
        {
            ServerContext.Parser = CommandParser.CreateNew().UsePrefix().OnError(OnParseError);
        }

        public static void CompileCommands()
        {
            ServerContext.Parser.AddCommand(Command
                .Create("Create Item") 
                .AddAlias("give")
                .SetAction(OnItemCreate)
                .AddArgument(Argument.Create("item"))
                .AddArgument(Argument.Create("amount").MakeOptional().SetDefault(1)));

            ServerContext.Parser.AddCommand(Command
                .Create("Create Monster")
                .AddAlias("cm")
                .SetAction(OnMonsterCreate)
                .AddArgument(Argument.Create("name"))
                .AddArgument(Argument.Create("image"))
                .AddArgument(Argument.Create("map"))
                .AddArgument(Argument.Create("x"))
                .AddArgument(Argument.Create("y"))
                .AddArgument(Argument.Create("level"))
                .AddArgument(Argument.Create("count"))
                .AddArgument(Argument.Create("offense").MakeOptional().SetDefault(ElementManager.Element.Random))
                .AddArgument(Argument.Create("defense").MakeOptional().SetDefault(ElementManager.Element.Random))
                .AddArgument(Argument.Create("script").MakeOptional().SetDefault("Common Monster"))
            );

            ServerContext.Parser.AddCommand(Command
                .Create("Add Map Block")
                .AddAlias("addblock")
                .SetAction(OnBlockAdded));

            ServerContext.Parser.AddCommand(Command
                .Create("Teleport")
                .AddAlias("tp")
                .SetAction(OnTeleport)
                .AddArgument(Argument.Create("map"))
                .AddArgument(Argument.Create("x"))
                .AddArgument(Argument.Create("y")));

            ServerContext.Parser.AddCommand(Command
                .Create("Summon Player")
                .AddAlias("sp")
                .SetAction(OnSummonPlayer)
                .AddArgument(Argument.Create("who")));

            ServerContext.Parser.AddCommand(Command
                .Create("Port to Player")
                .AddAlias("pt")
                .SetAction(OnPortToPlayer)
                .AddArgument(Argument.Create("who")));
        }

        /// <summary>
        /// In Game Usage : /cm test 1 3029 46 33 99 20 Dark Light "Common Monster"
        /// It represents the following structure: Name | Image | Map Id | X | Y | Spawn Count | Offense | Defense | Script Name
        /// </summary>
        private static void OnMonsterCreate(Argument[] args, object arg)
        {
            var client = (GameClient)arg;

            if (client == null)
                return;

            var template = new MonsterTemplate();
            {
                var name = args.FromName("name").Replace("\"", "");
                var image = args.FromName("image");
                var offsense = args.FromName("offense");
                var defense = args.FromName("defense");
                var level = args.FromName("level");
                var map = args.FromName("map");
                var x = args.FromName("x");
                var y = args.FromName("y");
                var script = args.FromName("Script").Replace("\"", "");
                var count = args.FromName("count");

                if (Position.TryParse(x, y, out var position))
                {
                    template.Image = ushort.Parse(image) < 0x4000
                        ? (ushort) (ushort.Parse(image) + 0x4000)
                        : ushort.Parse(image);
                    template.DefinedX = position.X;
                    template.DefinedY = position.Y;
                    template.ScriptName = script;
                    template.DefenseElement = Enum.Parse<ElementManager.Element>(defense);
                    template.OffenseElement = Enum.Parse<ElementManager.Element>(offsense);
                    template.UpdateMapWide = true;
                    template.EngagedWalkingSpeed = 1000;
                    template.PathQualifer = PathQualifer.Wander;
                    template.AreaID = int.Parse(map);
                    template.MovementSpeed = 1000;
                    template.AttackSpeed = 1000;
                    template.CastSpeed = 8000;
                    template.SpawnType = SpawnQualifer.Defined;
                    template.SpawnRate = 1;
                    template.BaseName = name;
                    template.Name = name;
                    template.MoodType = MoodQualifer.Unpredicable;
                    template.FamilyKey = "Minion";
                    template.Level = int.Parse(level);
                    template.LootType = LootQualifer.Gold | LootQualifer.Table;
                    template.Drops = new Collection<string>
                    {
                        "random"
                    };
                    template.SpawnMax = int.Parse(count);
                    if (ServerContext.GlobalMapCache.ContainsKey(template.AreaID))
                    {
                        StorageManager.MonsterBucket.Save(template, true);
                        ServerContext.GlobalMonsterTemplateCache.Add(template);
                    }
                }
            }
        }

        /// <summary>
        /// In Game Usage: /addblock
        /// </summary>
        private static void OnBlockAdded(Argument[] args, object arg)
        {
            var client = (GameClient) arg;

            if (client == null)
                return;

            if (!ServerContext.GlobalMapCache.ContainsKey(client.Aisling.CurrentMapId))
                return;

            var map = ServerContext.GlobalMapCache[client.Aisling.CurrentMapId];
            map?.AddBlock(client.Aisling.Position);
        }

        /// <summary>
        /// InGame Usage : /sp "Wren"
        /// </summary>
        private static void OnSummonPlayer(Argument[] args, object arg)
        {
            var client = (GameClient) arg;

            if (client != null)
            {
                var who = args.FromName("who").Replace("\"", "");

                if (string.IsNullOrEmpty(who)) 
                    return;

                var player = client.Server.Clients.FirstOrDefault(i =>
                    i?.Aisling != null && i.Aisling.Username.ToLower() == who.ToLower());

                //summon player to my map and position.
                player?.TransitionToMap(client.Aisling.Map, client.Aisling.Position);
            }
        }

        /// <summary>
        /// InGame Usage : /pt "Wren"
        /// </summary>
        private static void OnPortToPlayer(Argument[] args, object arg)
        {
            var client = (GameClient)arg;

            if (client != null)
            {
                var who = args.FromName("who").Replace("\"", "");

                if (string.IsNullOrEmpty(who)) 
                    return;

                var player = client.Server.Clients.FirstOrDefault(i =>
                    i?.Aisling != null && i.Aisling.Username.ToLower() == who.ToLower());

                //summon myself to players area and position.
                if (player != null)
                    client.TransitionToMap(player.Aisling.Map, player.Aisling.Position);
            }
        }

        /// <summary>
        /// InGame Usage : /tp "Abel Dungeon 2-1" 35 36
        /// </summary>
        private static void OnTeleport(Argument[] args, object arg)
        {
            var client = (GameClient) arg;

            if (client != null)
            {
                var mapName = args.FromName("map").Replace("\"", "");

                if (!int.TryParse(args.FromName("x"), out var x) ||
                    !int.TryParse(args.FromName("y"), out var y)) return;

                var (_, area) = ServerContext.GlobalMapCache.FirstOrDefault(i => i.Value.Name == mapName);

                if (area != null)
                {
                    client.TransitionToMap(area, new Position(x, y));
                }
            }
        }

        /// <summary>
        /// InGame Usage : /give "Dark Belt" 3
        /// InGame Usage : /give "Raw Beryl" 33
        /// InGame Usage : /give "Hy-Brasyl Battle Axe" 
        /// </summary>
        private static void OnItemCreate(Argument[] args, object arg)
        {
            var client = (GameClient) arg;

            if (client != null)
            {
                var name = args.FromName("item").Replace("\"", "");
                if (int.TryParse(args.FromName("amount"), out var quantity))
                {
                    if (ServerContext.GlobalItemTemplateCache.ContainsKey(name))
                    {
                        var template = ServerContext.GlobalItemTemplateCache[name];
                        if (template.CanStack)
                        {
                            var stacks = quantity / template.MaxStack;
                            var remaining = quantity % template.MaxStack;

                            for (var i = 0; i < stacks; i++)
                            {
                                {
                                    var item = Item.Create(client.Aisling, template);
                                    item.Stacks = template.MaxStack;
                                    item.GiveTo(client.Aisling, false);
                                }
                            }

                            if (remaining > 0)
                            {
                                {
                                    var item = Item.Create(client.Aisling, template);
                                    item.Stacks = (ushort) remaining;
                                    item.GiveTo(client.Aisling, false);
                                }
                            }
                        }
                        else
                        {
                            for (var i = 0; i < quantity; i++)
                            {
                                var item = Item.Create(client.Aisling, template);
                                item.GiveTo(client.Aisling, false);
                            }
                        }
                    }
                }
            }
        }

        public static void ParseChatMessage(IGameClient client, string message) => ServerContext.Parser?.Parse(message, client);

        public static void OnParseError(object obj, string command) =>
            ServerContext.Logger?.Invoke($"[Chat Parser] Error: {command}");
    }
}
