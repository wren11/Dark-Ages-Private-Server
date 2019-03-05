using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;
using MenuInterpreter;
using MenuInterpreter.Parser;
using System.Collections.Generic;
using System.Linq;

namespace Darkages.Storage.locales.Scripts.Menus
{
    [Script("Test Menu Script")]
    public class TestMenu : MenuScript
    {
        YamlMenuParser parser = new YamlMenuParser();

        #region Constructor
        public TestMenu(GameServer server, Sprite lpsprite) : base(server, lpsprite)
        {

        }
        #endregion


        private void SetupInterpreter(GameClient client)
        {
            //Client should only be interacting with one menu at a time, so let's check if it's null or finished.
            //if so, give them a new interpreter.        
            if (client.MenuInterpter == null || client.MenuInterpter.IsFinished)
            {
                client.MenuInterpter = parser.CreateInterpreterFromFile(@"C:\Users\dm882\Documents\GitHub\DarkAges-Lorule-Server\Staging\bin\Release\Storage\locales\Scripts\Menus\TestMenu.yaml");
            }

            //register checkpoint function
            client.MenuInterpter.RegisterCheckpointHandler("HasItem", (sender, a) =>
            {
                ServerContext.Info.Debug($"Here we can check if player has {a.Amount} {a.Value} in his inventory.");
                a.Result = true;
            });
        }

        //this function sends the game client the packet to display the current menu or sequence.
        public void ShowCurrentMenu(GameClient client, Sprite obj, MenuItem currentitem, MenuItem nextitem)
        {
            //no next item here.
            if (nextitem == null)
            {
                client.MenuInterpter = null;
                return;
            }


            //build step and send to client
            if (nextitem.Type == MenuItemType.Step)
            {
                client.Send(new ReactorSequence(client, new DialogSequence()
                {
                    DisplayText = nextitem.Text,
                    HasOptions = false,
                    DisplayImage = (ushort)(obj as Mundane).Template.Image,
                    Title = nextitem.Text,
                    CanMoveNext = nextitem.Answers.Length > 0,
                    CanMoveBack = nextitem.Answers.Any(i => i.Text == "back"),
                    Id = obj.Serial,
                }));
            }
            //build menu and send to client
            else if (nextitem.Type == MenuItemType.Menu)
            {
                var options = new List<OptionsDataItem>();

                foreach (var ans in nextitem.Answers)
                {
                    //dont include close in client display.
                    if (ans.Text == "close")
                        continue;

                    options.Add(new OptionsDataItem((short)ans.Id, ans.Text));
                    ServerContext.Info.Debug($"{ans.Id}. {ans.Text}");
                }

                client.SendOptionsDialog(obj as Mundane, nextitem.Text, options.ToArray());
            }
            else if (nextitem.Type == MenuItemType.Checkpoint)
            {
                //hm?
                client.MenuInterpter.Move(0);
            }
        }

        /// <summary>
        /// This handler fires when the user has clicked the NPC in-game.
        /// </summary>
        /// <param name="client">The client that performed the click.</param>
        /// <param name="obj">the sprite object (NPC)</param>
        public override void On_NPC_Clicked(GameClient client, Sprite obj)
        {
            SetupInterpreter(client);

            var interpreter = client.MenuInterpter;
            var currentStep = interpreter.GetCurrentStep();

            ShowCurrentMenu(client, obj, null, currentStep);
        }

        /// <summary>
        /// This handler fires when the user has clicked Back button/answer on a step sequence of menutype 'Step'
        /// </summary>
        /// <param name="client">The client that performed the click.</param>
        /// <param name="obj">the sprite object (NPC)</param>
        public override void On_Step_Answer_Back(GameClient client, Sprite obj)
        {
            var interpreter = client.MenuInterpter;
            var back        = interpreter.GetCurrentStep().Answers.FirstOrDefault(i => i.Text == "back");

            if (back != null)
            {
                ShowCurrentMenu(client, obj, interpreter.GetCurrentStep(), interpreter.Move(back.Id));
            }
            else
            {
                client.CloseDialog();
            }
        }


        /// <summary>
        /// This handler fires when the user has clicked the Next button/answer on a step sequence of menutype 'Step'
        /// </summary>
        /// <param name="client">The client that performed the click.</param>
        /// <param name="obj">the sprite object (NPC)</param>
        public override void On_Step_Answer_Next(GameClient client, Sprite obj)
        {
            var interpreter = client.MenuInterpter;
            var next        = interpreter.GetCurrentStep().Answers.FirstOrDefault(i => i.Text == "next");

            if (next != null)
                ShowCurrentMenu(client, obj, interpreter.GetCurrentStep(), interpreter.Move(next.Id));
            else
            {
                var complete = interpreter.GetCurrentStep().Answers.FirstOrDefault(i => i.Text == null);

                if (complete != null)
                {
                    ShowCurrentMenu(client, obj, null, interpreter.Move(complete.Id));
                }
            }
        }

        /// <summary>
        /// This handler fires when the user has clicked the Close button/answer on a step sequence of menutype 'Step'
        /// </summary>
        /// <param name="client">The client that performed the click.</param>
        /// <param name="obj">the sprite object (NPC)</param>
        public override void On_Step_Answer_Closed(GameClient client, Sprite obj)
        {
            var interpreter = client.MenuInterpter;
            var step = interpreter.GetCurrentStep();

            if (step == null)
            {
                client.MenuInterpter = null;
                //reset interpreter
                return;
            }

            var close = step.Answers.FirstOrDefault(i => i.Text == "close");

            if (close != null)
            {
                ShowCurrentMenu(client, obj, interpreter.GetCurrentStep(), interpreter.Move(close.Id));
                client.CloseDialog();
            }
        }

        /// <summary>
        /// This handler fires when the user has clicked on one of the answers prompted bya menu of menutype 'Menu'
        /// </summary>
        /// <param name="client">The client that picked an answer</param>
        /// <param name="selected_answer">The answer that was picked.</param>
        /// <param name="obj">the sprite object (NPC)</param>
        public override void On_Menu_Answer_Clicked(GameClient client, Sprite obj, MenuInterpreter.Answer selected_answer)
        {
            var interpreter = client.MenuInterpter;

            if (selected_answer.Id > 100)
            {
                client.MenuInterpter = null;
            }

            ShowCurrentMenu(client, obj, null, interpreter.Move(selected_answer.Id));
        }
    }
}
