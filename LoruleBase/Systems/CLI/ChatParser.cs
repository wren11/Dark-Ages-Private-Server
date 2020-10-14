using System.Linq;
using Darkages.Network.Game;
using Darkages.Types;
using Pyratron.Frameworks.Commands.Parser;

namespace Darkages.Systems.CLI
{
    public class ChatParser
    {
        public static CommandParser Parser { get; set; }

        static ChatParser()
        {
            Parser = CommandParser.CreateNew().UsePrefix().OnError(OnParseError);
            CompileCommands();
        }

        public static void CompileCommands()
        {
            Parser.AddCommand(Command
                .Create("Create Item")
                .AddAlias("give")
                .SetAction(OnItemCreate)
                .AddArgument(Argument.Create("item"))
                .AddArgument(Argument.Create("amount").MakeOptional().SetDefault(1)));

            Parser.AddCommand(Command
                .Create("Teleport")
                .AddAlias("tp")
                .SetAction(OnTeleport)
                .AddArgument(Argument.Create("map"))
                .AddArgument(Argument.Create("x"))
                .AddArgument(Argument.Create("y")));
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

        public static void ParseChatMessage(IGameClient client, string message) => Parser?.Parse(message, client);

        public static void OnParseError(object obj, string command) =>
            ServerContext.Logger?.Invoke($"[Chat Parser] Error: {command}");
    }
}
