#region

using System;
using System.Linq;

#endregion

namespace Darkages.Types
{
    public class QuestRequirement
    {
        public int Amount { get; set; }
        public Template TemplateContext { get; set; }
        public QuestType Type { get; set; }
        public string Value { get; set; }

        public bool IsMet(Aisling user, Func<Predicate<Template>, bool> predicate)
        {
            if (user.GameMaster)
                return true;

            return Type switch
            {
                QuestType.ItemHandIn => predicate(i => user.Inventory.Has(TemplateContext) >= Amount),
                QuestType.KillCount => predicate(i => user.HasKilled(Value, Amount)),
                QuestType.HasItem => predicate(i => user.Inventory.HasCount(TemplateContext) >= Amount),
                QuestType.SingleItemHandIn => predicate(i => user.Inventory.HasCount(TemplateContext) >= Amount),
                QuestType.Gossip => true,
                _ => false
            };
        }
    }
}