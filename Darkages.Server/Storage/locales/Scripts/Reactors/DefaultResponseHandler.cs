namespace Darkages.Storage.locales.Scripts.Reactors
{
    using global::Darkages.Scripting;
    using global::Darkages.Types;
    using System;
    using System.Linq;

    namespace Darkages.Assets.locales.Scripts.Reactors
    {
        [Script("Default Response Handler")]
        public class ExampleReactor2 : ReactorScript
        {
            public ExampleReactor2(Reactor reactor) : base(reactor)
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

                if (aisling.ActiveReactor.Index + 1 < aisling.ActiveReactor.Steps.Count)
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
                if (aisling.ReactedWith(Reactor.Name))
                {
                    foreach (var sequences in Reactor.Steps.Where(i => i.Callback != null))
                        sequences.Callback.Invoke(aisling, sequences);
                }
                else
                {
                    aisling.ReactorActive = true;
                    aisling.ActiveReactor = Reactor;
                    aisling.ActiveReactor.Next(aisling.Client);
                }
            }

            void SequenceComplete(Aisling aisling, DialogSequence sequence)
            {
                if (aisling.ReactedWith(Reactor.Name))
                    return;

                if (sequence == null)
                {
                    aisling.Reactions[Reactor.Name] = DateTime.UtcNow;
                    aisling.ReactorActive = false;
                    aisling.ActiveReactor = null;
                    aisling.Client.CloseDialog();

                    foreach (var sequences in Reactor.Steps.Where(i => i.Callback != null))
                        sequences.Callback.Invoke(aisling, sequences);

                    if (Reactor.Quest != null && Reactor.Quest.Completed)
                        Reactor.Quest.Rewards(aisling, false);

                    if (Reactor.PostScript != null)
                        Reactor.PostScript.OnTriggered(aisling);

                }
            }
        }
    }
}
