using Darkages.Network.Game;
using Darkages.Scripting;
using Darkages.Types;
using MenuInterpreter;
using MenuInterpreter.Parser;
using System.Collections.Generic;
using System.IO;

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script("tut/OldMan")]
    public class OldMan : MundaneScript
    {
        Quest quest = new Quest()
        {
            Name = "Bartley_tut1",
            GoldReward = 5000,
            StatRewards = new List<AttrReward>()
              {
                  new AttrReward()
                  {
                       Attribute = PlayerAttr.STR,
                       Operator = new StatusOperator(Operator.Add, 3),
                  },
              }

        };


        public void LoadScriptInterpreter(GameClient client)
        {
            var parser   = new YamlMenuParser();
            var yamlPath = ServerContext.StoragePath + string.Format(@"\Scripts\Menus\{0}.yaml", Mundane.Template.Name);

            if (File.Exists(yamlPath))
            {
                if (client.MenuInterpter == null)
                {
                    client.MenuInterpter = parser.CreateInterpreterFromFile(yamlPath);

                    client.MenuInterpter.Client = client;

                    client.MenuInterpter.OnMovedToNextStep += MenuInterpreter_OnMovedToNextStep;

                    client.MenuInterpter.RegisterCheckpointHandler("QuestCompleted", (_client, res) =>
                    {
                        if (_client.Aisling.HasQuest(res.Value))
                        {
                            res.Result = _client.Aisling.HasCompletedQuest(res.Value);
                        }

                    });

                    client.MenuInterpter.RegisterCheckpointHandler("HasKilled", (_client, res) =>
                    {
                        if (_client.Aisling.HasQuest(quest.Name))
                        {
                            if (_client.Aisling.HasKilled(res.Value, res.Amount))
                            {
                                if (!_client.Aisling.HasCompletedQuest(quest.Name))
                                {
                                    _client.Aisling.CompleteQuest(quest.Name);
                                    quest.OnCompleted(_client.Aisling, false);
                                }
                                res.Result = true;
                            }
                        }
                    });

                    ServerContext.Info.Debug("Script Interpreter Created for Mundane: {0}", Mundane.Template.Name);
                }
            }


        }

        public OldMan(GameServer server, Mundane mundane) : base(server, mundane)
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

            if (client.Aisling.AcceptQuest(quest))
            {
                //quest has been added to interpreter.
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
