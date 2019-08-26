using System.Collections.Generic;
using System.Linq;

namespace Darkages.Types
{
    public class QuestChain
    {
        private readonly List<Quest> Quests = new List<Quest>();

        private int Index { get; set; } = 0;

        public void AddQuest(Quest lpQuest)
        {
            Quests.Add(lpQuest);
        }

        public bool CurrentQuestCompleted => CurrentQuest.Completed;

        public Quest GetNextQuest => NextQuest;

        public bool AllQuestsCompleted => Quests.TrueForAll(i => i.Completed);

        public Quest FindQuest(string lpString)
        {
            return Quests.FirstOrDefault(i => i.Name.ToLower() == lpString.ToLower());
        }

        public bool IsQuestCompleted(string lpString)
        {
            var q = FindQuest(lpString);

            if (q != null)
            {
                return q.Completed;
            }

            return false;
        }

        public Quest CurrentQuest
        {
            get
            {
                if (Index > Quests.Count)
                    Index = 0;

                return Quests.ElementAt(Index);
            }
        }

        public Quest NextQuest
        {
            get
            {
                if (Index + 1 > Quests.Count)
                    return null;

                return Quests.ElementAt(Index + 1);
            }
        }

        public Quest First => Quests.FirstOrDefault();

        public void OntoNext()
        {
            if (Index + 1 < Quests.Count)
            {
                Index++;
            }
        }
    }
}
