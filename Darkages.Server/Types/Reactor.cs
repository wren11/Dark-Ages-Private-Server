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
        Map    = 0,
        Object = 1,
        Item   = 3,
        Skill  = 4,
        Spell  = 5
    }

    public class Reactor : Template
    {
        public string ScriptKey { get; set; }

        public string CallBackScriptKey { get; set; }

        public ReactorQualifer CallerType { get; set; }

        public int MapId { get; set; }

        public Position Location { get; set; }

        [JsonIgnore]
        public int Index { get; set; }

        [JsonIgnore]
        public ReactorScript Script { get; set; }

        [JsonIgnore]
        public ReactorScript PostScript { get; set; }

        public List<DialogSequence> Steps = new List<DialogSequence>();

        public void Update(GameClient client)
        {
            if (client.Aisling.ActiveReactor == null)
            {
                if (Script != null)
                    Script.OnTriggered(client.Aisling);
            }
        }

        public void Next(GameClient client, bool start = false)
        {
            if (Steps.Count == 0)
                return;

            if (!start)
            {
                client.Send(new ReactorSequence(client, Steps[Index]));
                return;
            }

            var first = Steps[Index = 0];
            if (first != null)
                client.Send(new ReactorSequence(client, first));
        }
    }
}
