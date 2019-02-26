using Darkages.Scripting;
using Darkages.Types;
using System;

namespace Darkages.Assets.locales.Scripts.Reactors
{
    [Script("example reactor")]
    public class ExampleReactor : ReactorScript
    {
        public ExampleReactor(Reactor reactor) : base(reactor)
        {
            Reactor 
                 = reactor;
        }

        public override void OnBack(Aisling aisling)
        {
            if (aisling.ActiveReactor == null)
            {
                aisling.ReactorActive = false;
                aisling.Client.CloseDialog();
                return;
            }

            if (aisling.ActiveReactor.Index - 1 >= 0)
            {
                aisling.ActiveReactor.Index--;
                aisling.ActiveReactor.Next(aisling.Client, true);
            }
        }

        public override void OnClose(Aisling aisling)
        {
            aisling.ReactorActive = false;
            aisling.ActiveReactor = null;
        }

        public override void OnNext(Aisling aisling)
        {

            if (aisling.ActiveReactor == null)
            {
                aisling.ReactorActive = false;
                aisling.Client.CloseDialog();
                return;
            }
            
            if (aisling.ActiveReactor.Index + 1 < aisling.ActiveReactor.Sequences.Count)
            {
                aisling.ActiveReactor.Index++;
                aisling.ActiveReactor.Next(aisling.Client);
            }
            else
            {
                aisling.ActiveReactor.Index--;
                SequenceComplete(aisling, null);
            }

        }

        public override void OnTriggered(Aisling aisling)
        {
            aisling.ReactorActive = true;
            aisling.ActiveReactor = Reactor;
            aisling.ActiveReactor.Next(aisling.Client);
        }

        void SequenceComplete(Aisling aisling, DialogSequence sequence)
        {
            if (sequence == null)
            {
                aisling.Reactions[Reactor.Name] = DateTime.UtcNow;
                aisling.ReactorActive = false;
                aisling.ActiveReactor = null;
                aisling.Client.CloseDialog();

                if (Reactor.Quest != null)
                    Reactor.Quest.Rewards(aisling, false);

                if (Reactor.PostScript != null)
                {
                    Reactor.PostScript.OnTriggered(aisling);
                }
            }
        }
    }
}
