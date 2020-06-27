using System;

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
            switch (Type)
            {
                case QuestType.ItemHandIn:
                    return predicate(i => user.Inventory.Has(TemplateContext) >= Amount);

                case QuestType.KillCount:
                    return predicate(i => user.HasKilled(Value, Amount));

                case QuestType.HasItem:
                    return predicate(i => user.Inventory.HasCount(TemplateContext) >= Amount);

                case QuestType.SingleItemHandIn:
                    return predicate(i => user.Inventory.HasCount(TemplateContext) >= Amount);

                case QuestType.Gossip:
                    return true;

                default:
                    return false;
            }
        }
    }
}