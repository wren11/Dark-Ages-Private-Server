#region

using System;
using System.IO;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;
using MenuInterpreter;
using MenuInterpreter.Parser;
using Microsoft.CodeAnalysis.CSharp.Scripting;

#endregion

namespace Darkages.Storage.locales.Scripts.Examples.YamlExamples
{
    [Script("MultiScriptNpc", "Dean", "An example of mixing yaml and cs.")]
    public class MultiScriptNpc : MundaneScript
    {
        //Setup the Main Menu
        private readonly OptionsDataItem[] _mainMenu =
        {
            new OptionsDataItem(0x0001, "Buy"),
            new OptionsDataItem(0x0002, "Sell"),
            new OptionsDataItem(0x0003, "Repair Items")
        };

        //Setup some items available to buy.
        private string[] _buyableItems =
        {
            "Dark Belt",
            "Light Belt",
            "Holy Diana",
            "Magus Diana"
        };

        private bool reset;

        public MultiScriptNpc(GameServer server, Mundane mundane) : base(server, mundane)
        {
        }

        public Interpreter LoadScriptInterpreter(GameClient client, OptionsDataItem item)
        {
            var parser = new YamlMenuParser();
            var yamlPath = ServerContext.StoragePath + $@"\Interactive\Menus\{GetType().Name}\{item.Text}.yaml";

            var globals = new ScriptGlobals
            {
                actor = Mundane,
                client = client,
                user = client.Aisling
            };

            if (File.Exists(yamlPath))
                if (client.MenuInterpter == null)
                {
                    client.MenuInterpter = parser.CreateInterpreterFromFile(yamlPath);

                    client.MenuInterpter.Client = client;

                    client.MenuInterpter.OnMovedToNextStep += MenuInterpreter_OnMovedToNextStep;

                    client.MenuInterpter.RegisterCheckpointHandler($"On{item.Text}", (c, res) => { });

                    client.MenuInterpter.RegisterCheckpointHandler("Call", async (c, res) =>
                    {
                        try
                        {
                            await CSharpScript.EvaluateAsync<bool>(res.Value, GameServer.ScriptOptions, globals);
                            res.Result = globals.result;
                            reset = true;
                        }
                        catch (Exception e)
                        {
                            ServerContext.Logger($"Script Error: {res.Value}.");
                            ServerContext.Error(e);

                            res.Result = false;
                        }
                    });
                }

            return client.MenuInterpter;
        }

        public override void OnClick(GameServer server, GameClient client)
        {
            //user clicked me, let's show them the main menu.
            client.SendOptionsDialog(Mundane, "What you looking for?", _mainMenu);
        }

        public override void OnGossip(GameServer server, GameClient client, string message)
        {
        }

        //This is called when something from the main menu was clicked.
        public override void OnResponse(GameServer server, GameClient client, ushort responseId, string args)
        {
            if (reset) return;

            if (_mainMenu.Length > responseId)
            {
                if (client.MenuInterpter == null)
                    client.MenuInterpter = LoadScriptInterpreter(client, _mainMenu[responseId - 1]);

                client.MenuInterpter.Start();
            }
            else
            {
                //user clicked me, let's show them the main menu.
                client.SendOptionsDialog(Mundane, "What you looking for?", _mainMenu);
            }

            if (Mundane != null) client.ShowCurrentMenu(Mundane, null, client.MenuInterpter.GetCurrentStep());
        }

        public override void TargetAcquired(Sprite target)
        {
        }

        private void MenuInterpreter_OnMovedToNextStep(GameClient client, MenuItem previous, MenuItem current)
        {
        }
    }
}