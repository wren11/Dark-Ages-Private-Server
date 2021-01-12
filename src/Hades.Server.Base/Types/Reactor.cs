#region

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Darkages.Common;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;


#endregion

namespace Darkages.Types
{
    [Serializable]
    public class Reactor : Template
    {
        [JsonIgnore] public readonly int Id;

        public List<DialogSequence> Sequences = new List<DialogSequence>();

        public Reactor()
        {
            lock (Generator.Random)
            {
                Id = Generator.GenerateNumber();
            }
        }

        public string CallBackScriptKey { get; set; }
        public ReactorQualifer CallerType { get; set; }
        public string CallingReactor { get; set; }
        public bool CanActAgain { get; set; }
        [JsonIgnore] public DialogSequence Current => Sequences[Index] ?? null;
        [JsonIgnore] public Dictionary<string, ReactorScript> Decorators { get; set; }
        [JsonIgnore] public int Index { get; set; }
        public Position Location { get; set; }
        public Quest Quest { get; set; }
        public string ScriptKey { get; set; }

        [ObsoleteAttribute("This property is obsolete. and should not be used.", false)]
        public GameServerTimer WhenCanActAgain { get; set; }

        [ObsoleteAttribute("This property is obsolete. and should not be used.", false)]
        public int MapId { get; set; }

        [ObsoleteAttribute("This property is obsolete. and should not be used.", false)]
        [JsonIgnore]
        public ReactorScript PostScript { get; set; }

        [ObsoleteAttribute("This property is obsolete. and should not be used.", false)]
        public string DecoratorScript { get; set; }

        [ObsoleteAttribute("This property is obsolete. and should not be used.", false)]
        public bool Completed { get; set; }

        [ObsoleteAttribute("This property is obsolete. and should not be used.", false)]
        public string CallingNpc { get; set; }


        public void Goto(GameClient client, int Idx)
        {
            client.Aisling.ActiveReactor.Index = Idx;
            client.Aisling.ActiveSequence = client.Aisling.ActiveReactor.Sequences[Idx];

            client.Send(new ReactorSequence(client, client.Aisling.ActiveSequence));

            if (Sequences[Idx].OnSequenceStep != null)
                Sequences[Idx].OnSequenceStep.Invoke(client.Aisling, Sequences[Idx]);
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
                    var mundane = GetObject<Mundane>(client.Aisling.Map,
                        i => i.WithinRangeOf(client.Aisling) && i.Alive);

                    if (client.Aisling.ActiveSequence.HasOptions && client.Aisling.ActiveSequence.Options.Length > 0)
                    {
                        if (mundane != null)
                            client.SendOptionsDialog(mundane,
                                client.Aisling.ActiveSequence.DisplayText,
                                client.Aisling.ActiveSequence.Options);
                    }
                    else if (client.Aisling.ActiveSequence.IsCheckPoint)
                    {
                        var results = new List<bool>();
                        var valid = false;

                        if (client.Aisling.ActiveSequence.Conditions != null)
                        {
                            foreach (var reqs in client.Aisling.ActiveSequence.Conditions)
                                results.Add(reqs.IsMet(client.Aisling, i => i(reqs.TemplateContext)));

                            valid = results.TrueForAll(i => i);
                        }
                        else
                        {
                            valid = true;
                        }

                        if (valid)
                            Goto(client, Index);
                        else
                            client.SendOptionsDialog(mundane, client.Aisling.ActiveSequence.ConditionFailMessage,
                                "failed");
                    }
                    else
                    {
                        Goto(client, Index);
                    }

                    if (Sequences[Index].OnSequenceStep != null)
                        Sequences[Index].OnSequenceStep.Invoke(client.Aisling, Sequences[Index]);
                }

                return;
            }

            var first = Sequences[Index = 0];
            if (first != null) client.Send(new ReactorSequence(client, first));
        }

        public void Update(GameClient client)
        {
            if (client.Aisling.CanReact)
            {
                client.Aisling.CanReact = false;

                if (Decorators != null)
                    foreach (var script in Decorators.Values)
                        script?.OnTriggered(client.Aisling);
            }
        }

        public override string[] GetMetaData()
        {
            return new[]
            {
                ""
            };
        }
    }
}