#region

using System;
using System.Collections.Generic;
using Darkages.Network.ServerFormats;
using Newtonsoft.Json;

#endregion

namespace Darkages.Types
{
    public class DialogSequence
    {
        public string CallbackKey { get; set; }
        public bool CanMoveBack { get; set; }
        public bool CanMoveNext { get; set; }
        public string ConditionFailMessage { get; set; }
        public List<QuestRequirement> Conditions { get; set; }
        public int ContinueAt { get; set; }
        public short ContinueOn { get; set; }
        public ushort DisplayImage { get; set; }
        public string DisplayText { get; set; }
        public bool HasOptions { get; set; }
        public int Id { get; set; }
        public bool IsCheckPoint { get; set; }
        [JsonIgnore] public Action<Aisling, DialogSequence> OnSequenceStep { get; set; }
        public OptionsDataItem[] Options { get; set; }
        public int RollBackTo { get; set; }
        public string Title { get; set; }
    }
}