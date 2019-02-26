namespace Darkages.Storage.locales.Scripts.Reactors
{
    using global::Darkages.Scripting;
    using global::Darkages.Types;
    using System;

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
                    aisling.ReactorActive  = false;
                    aisling.ActiveSequence = null;
                    aisling.Client.CloseDialog();
                    return;
                }

                if (aisling.ActiveReactor.Index - 1 >= 0)
                {
                    aisling.ActiveReactor.Index--;
                    aisling.ActiveReactor.Goto(aisling.Client, aisling.ActiveReactor.Index);
                }
            }

            public override void OnClose(Aisling aisling)
            {
                aisling.ActiveReactor = null;
                aisling.ReactorActive = false;
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
                    aisling.ActiveSequence = aisling.ActiveReactor.Sequences[aisling.ActiveReactor.Index];
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
                    if (Reactor.CanActAgain)
                    {
                        aisling.Reactions.Remove(Reactor.Name);

                        aisling.ReactorActive = true;
                        aisling.ActiveReactor.Next(aisling.Client, true);
                    }
                }
                else
                {
                    if (aisling.ActiveReactor != null)
                    {
                        aisling.ReactorActive  = true;
                        aisling.ActiveSequence = aisling.ActiveReactor.Sequences[0];
                        aisling.ActiveReactor.Next(aisling.Client);
                    }
                }
            }

            void SequenceComplete(Aisling aisling, DialogSequence sequence)
            {
                if (aisling.ActiveSequence != null && !aisling.ActiveSequence.CanMoveNext)
                {
                    return;
                }

                if (aisling.ReactedWith(Reactor.Name))
                    return;

                if (sequence == null)
                {
                    aisling.Reactions[Reactor.Name] = DateTime.UtcNow;
                    aisling.ReactorActive = false;
                    aisling.ActiveSequence = null;
                    aisling.ActiveReactor.Completed = true;

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
                }
            }
        }
    }
}
