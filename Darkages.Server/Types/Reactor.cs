using Darkages.Common;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Darkages.Types
{
    public enum ReactorQualifer
    {
        Map = 0,
        Object = 1,
        Item = 3,
        Skill = 4,
        Spell = 5,
        Reactor = 6,
        Quest = 7
    }

    public class Reactor : Template
    {
        [JsonIgnore]
        public readonly int Id;

        public Reactor NextReactor { get; set; }

        public Reactor()
        {
            lock (Generator.Random)
            {
                Id = Generator.GenerateNumber();
            }
        }

        public string ScriptKey { get; set; }

        public string CallBackScriptKey { get; set; }

        public ReactorQualifer CallerType { get; set; }

        public int MapId { get; set; }

        public string CallingReactor { get; set; }

        public Position Location { get; set; }

        public Quest Quest { get; set; }

        [JsonIgnore]
        public int Index { get; set; }

        [JsonIgnore]
        public DialogSequence Current => Steps[Index] ?? null;

        [JsonIgnore]
        public ReactorScript Script { get; set; }

        [JsonIgnore]
        public ReactorScript PostScript { get; set; }

        public bool CanActAgain { get; set; }

        public bool Completed { get; set; }

        public List<DialogSequence> Steps = new List<DialogSequence>();

        public void Update(GameClient client)
        {
            if (client.Aisling.CanReact)
            {
                client.Aisling.CanReact = false;

                if (Script != null)
                {
                    Script.OnTriggered(client.Aisling);
                }
            }
        }

        public void Goto(GameClient client, int Idx)
        {
            client.Aisling.ActiveReactor.Index = Idx;
            client.Aisling.ActiveSequence      = client.Aisling.ActiveReactor.Steps[Idx];

            client.Send(new ReactorSequence(client, client.Aisling.ActiveSequence));

            if (Steps[Idx].Callback != null)
            {
                Steps[Idx].Callback.Invoke(client.Aisling, Steps[Idx]);
            }
        }

        public void Next(GameClient client, bool start = false)
        {
            if (Steps.Count == 0)
                return;

            if (Index < 0)
                Index = 0;

            if (!start)
            {
                client.Send(new ReactorSequence(client, Steps[Index]));

                if (Steps[Index].Callback != null)
                {
                    Steps[Index].Callback.Invoke(client.Aisling, Steps[Index]);
                }

                return;
            }

            var first = Steps[Index = 0];
            if (first != null)
            {
                client.Send(new ReactorSequence(client, first));
            }
        }
    }
}
