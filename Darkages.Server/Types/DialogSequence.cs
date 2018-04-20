using System;

namespace Darkages.Types
{
    public class DialogSequence
    {
        public Action<Aisling, DialogSequence> Callback = null;
        public string Title { get; set; }
        public string DisplayText { get; set; }
        public bool HasOptions { get; set; }
        public bool StartsQuest { get; set; }
    }
}