namespace Darkages.Storage.locales.Scripts.Reactors
{
    using global::Darkages.Scripting;
    using global::Darkages.Types;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    namespace Darkages.Assets.locales.Scripts.Reactors
    {
        [Script("Default Response Handler")]
        public class DefaultReactor : ReactorScript
        {
            public DefaultReactor(Reactor reactor) : base(reactor)
            {
                Reactor = reactor;
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
                    foreach (var sequences in Reactor.Steps.Where(i => i.Callback != null && !i.Processed))
                    {
                        sequences.Callback.Invoke(aisling, sequences);
                    }

                    if (Reactor.CanActAgain)
                    {
                        aisling.Reactions.Remove(Reactor.Name);
                    }
                }
                else
                {
                    if (aisling.ActiveReactor != null)
                    {
                        aisling.ReactorActive = true;
                        aisling.ActiveReactor.Next(aisling.Client);
                    }
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

                    if (Reactor.Quest != null && !Reactor.Quest.Completed)
                    {
                        var aisling_quest = aisling.Quests.Find(i => i.Name == Reactor.Quest.Name);

                        if (aisling_quest == null)
                        {
                            aisling.AcceptQuest(Reactor.Quest);
                            aisling_quest = aisling.Quests.Find(i => i.Name == Reactor.Quest.Name);
                        }

                        if (!aisling_quest.Completed)
                        {
                            aisling_quest.HandleQuest(aisling.Client, null, quest_completed_ok =>
                            {
                                if (quest_completed_ok)
                                {
                                    aisling_quest.Completed = true;
                                    aisling_quest.OnCompleted(aisling, true);
                                }
                            });
                        }
                    }

                    if (Reactor.PostScript != null)
                        Reactor.PostScript.OnTriggered(aisling);

                }
            }
        }
    }
}
