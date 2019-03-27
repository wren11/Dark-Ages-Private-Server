using Darkages.Network.Game;
using Darkages.Scripting;
using Darkages.Types;
using MenuInterpreter;
using MenuInterpreter.Parser;
using System.IO;

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script("simple_generic")]
    public class simple_generic : MundaneScript
    {
        public void LoadScriptInterpreter(GameClient client)
        {
            var parser = new YamlMenuParser();
            var yamlPath = ServerContext.StoragePath + string.Format(@"\Scripts\Menus\{0}.yaml", Mundane.Template.Name);

            if (File.Exists(yamlPath))
            {
                if (client.MenuInterpter == null)
                {
                    client.MenuInterpter = parser.CreateInterpreterFromFile(yamlPath);
                    client.MenuInterpter.Client = client;

                    client.MenuInterpter.OnMovedToNextStep += MenuInterpreter_OnMovedToNextStep;

                    ServerContext.Info.Debug("Script Interpreter Created for Mundane: {0}", Mundane.Template.Name);
                }
            }


        }

        public simple_generic(GameServer server, Mundane mundane) : base(server, mundane)
        {

        }

        public void MenuInterpreter_OnMovedToNextStep(GameClient client, MenuItem previous, MenuItem current)
        {
            if (client.MenuInterpter != null)
            {
                if (client.MenuInterpter.IsFinished)
                {

                }
            }
        }

        public override void OnResponse(GameServer server, GameClient client, ushort responseID, string args)
        {

        }

        public override void OnClick(GameServer server, GameClient client)
        {
            if (client.MenuInterpter == null)
            {
                LoadScriptInterpreter(client);
                client.MenuInterpter.Start();
            }

            client.ShowCurrentMenu(Mundane, null, client.MenuInterpter.GetCurrentStep());
        }

        public override void TargetAcquired(Sprite Target)
        {

        }

        public override void OnGossip(GameServer server, GameClient client, string message)
        {

        }
    }    
}
