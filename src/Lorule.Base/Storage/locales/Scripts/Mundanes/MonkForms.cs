#region

using System.Collections.Generic;
using System.IO;
using Darkages.Network.Game;
using Darkages.Scripting;
using Darkages.Types;
using MenuInterpreter;
using MenuInterpreter.Parser;

#endregion

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script("monk/forms")]
    public class MonkForms : MundaneScript
    {
        private readonly Quest quest = new Quest
        {
            Name = "monk_forms_a",
            GoldReward = 5000,
            StatRewards = new List<AttrReward>
            {
                new AttrReward
                {
                    Attribute = PlayerAttr.STR,
                    Operator = new StatusOperator(Operator.Add, 3)
                }
            }
        };

        public MonkForms(GameServer server, Mundane mundane) : base(server, mundane)
        {
        }

        public void LoadScriptInterpreter(GameClient client)
        {
            var parser = new YamlMenuParser();
            var yamlPath = ServerContext.StoragePath + $@"\Scripts\Menus\{Mundane.Template.Name}.yaml";

            if (File.Exists(yamlPath))
                if (client.MenuInterpter == null)
                {
                    client.MenuInterpter = parser.CreateInterpreterFromFile(yamlPath);

                    client.MenuInterpter.Client = client;

                    client.MenuInterpter.OnMovedToNextStep += MenuInterpreter_OnMovedToNextStep;

                    client.MenuInterpter.RegisterCheckpointHandler("QuestCompleted", (_client, res) =>
                    {
                        if (_client.Aisling.HasQuest(res.Value))
                            res.Result = _client.Aisling.HasCompletedQuest(res.Value);
                    });

                    client.MenuInterpter.RegisterCheckpointHandler("HasAForm",
                        (_client, res) => { res.Result = _client.Aisling.AnimalForm != AnimalForm.None; });

                    client.MenuInterpter.RegisterCheckpointHandler("LearnForm1",
                        (_client, res) => { _client.Aisling.AnimalForm = AnimalForm.Draco; });

                    client.MenuInterpter.RegisterCheckpointHandler("LearnForm2",
                        (_client, res) => { _client.Aisling.AnimalForm = AnimalForm.Kelberoth; });

                    client.MenuInterpter.RegisterCheckpointHandler("LearnForm3",
                        (_client, res) => { _client.Aisling.AnimalForm = AnimalForm.WhiteBat; });

                    client.MenuInterpter.RegisterCheckpointHandler("LearnForm4",
                        (_client, res) => { _client.Aisling.AnimalForm = AnimalForm.Scorpion; });

                    client.MenuInterpter.RegisterCheckpointHandler("HasKilled", (_client, res) =>
                    {
                        if (_client.Aisling.HasQuest(quest.Name))
                            if (_client.Aisling.HasKilled(res.Value, res.Amount))
                            {
                                if (!_client.Aisling.HasCompletedQuest(quest.Name))
                                {
                                    _client.Aisling.CompleteQuest(quest.Name);
                                    quest.OnCompleted(_client.Aisling);
                                }

                                res.Result = true;
                            }
                    });

                    ServerContext.Logger($"Script Interpreter Created for Mundane: {Mundane.Template.Name}");
                }
        }

        public void MenuInterpreter_OnMovedToNextStep(GameClient client, MenuItem previous, MenuItem current)
        {
            if (client.MenuInterpter != null)
                if (client.MenuInterpter.IsFinished)
                {
                }
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
            }

            client.ShowCurrentMenu(Mundane, null, client.MenuInterpter.GetCurrentStep());
        }

        public override void OnGossip(GameServer server, GameClient client, string message)
        {
        }

        public override void OnResponse(GameServer server, GameClient client, ushort responseID, string args)
        {
        }

        public override void TargetAcquired(Sprite Target)
        {
        }
    }
}