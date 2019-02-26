using Darkages.Common;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

    [Serializable]
    public class Reactor : Template
    {
        [JsonIgnore]
        public readonly int Id;


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
        public DialogSequence Current => Sequences[Index] ?? null;

        [JsonIgnore]
        public ReactorScript Decorator { get; set; }

        public string DecoratorScript { get; set; }

        [JsonIgnore]
        public ReactorScript PostScript { get; set; }

        public bool CanActAgain { get; set; }

        public bool Completed { get; set; }

        public GameServerTimer WhenCanActAgain { get; set; }

        public List<DialogSequence> Sequences = new List<DialogSequence>();

        public string CallingNpc { get; set; }

        public void Update(GameClient client)
        {
            if (client.Aisling.CanReact)
            {
                client.Aisling.CanReact = false;

                if (Decorator != null)
                {
                    Decorator.OnTriggered(client.Aisling);
                }
            }
        }

        public void Goto(GameClient client, int Idx)
        {
            client.Aisling.ActiveReactor.Index  = Idx;
            client.Aisling.ActiveSequence       = client.Aisling.ActiveReactor.Sequences[Idx];

            client.Send(new ReactorSequence(client, client.Aisling.ActiveSequence));

            if (Sequences[Idx].OnSequenceStep != null)
            {
                Sequences[Idx].OnSequenceStep.Invoke(client.Aisling, Sequences[Idx]);
            }
        }

        public void Next(GameClient client, bool start = false)
        {
            if (Sequences.Count == 0)
                return;

            if (Index < 0)
                Index = 0;

            if (!start)
            {
                if (client.Aisling.ActiveSequence != null)
                {

                    var mundane = GetObject<Mundane>(client.Aisling.Map, i => i.WithinRangeOf(client.Aisling) && i.Alive);

                    if (client.Aisling.ActiveSequence.HasOptions && client.Aisling.ActiveSequence.Options.Length > 0)
                    {
                        if (mundane != null)
                        {
                            client.SendOptionsDialog(mundane,
                                client.Aisling.ActiveSequence.DisplayText,
                                client.Aisling.ActiveSequence.Options);
                        }
                    }
                    else if (client.Aisling.ActiveSequence.IsCheckPoint)
                    {
                        var results = new List<bool>();
                        var valid   = false;

                        if (client.Aisling.ActiveSequence.Conditions != null)
                        {

                            foreach (var reqs in client.Aisling.ActiveSequence.Conditions)
                            {
                                results.Add(reqs.IsMet(client.Aisling, i => i(reqs.TemplateContext)));
                            }

                            valid = results.TrueForAll(i => i != false);
                        }
                        else
                        {
                            valid = true;
                        }

                        if (valid)
                        {
                            Goto(client, Index); //send the next dialog.
                        }
                        else
                        {
                            client.SendOptionsDialog(mundane, client.Aisling.ActiveSequence.ConditionFailMessage, "failed");
                        }
                    }
                    else
                    {
                        Goto(client, Index); //send the next dialog.
                    }

                    if (Sequences[Index].OnSequenceStep != null)
                    {
                        Sequences[Index].OnSequenceStep.Invoke(client.Aisling, Sequences[Index]);
                    }
                }

                return;
            }

            var first = Sequences[Index = 0];
            if (first != null)
            {
                client.Send(new ReactorSequence(client, first));
            }
        }
    }
}
