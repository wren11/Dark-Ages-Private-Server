using Darkages.Systems.Loot.Interfaces;
using Darkages.Types;
using System.Collections.Generic;

namespace Darkages.Systems.Loot
{
    public class LootModifierSet : Template, IModifierSet
    {
        public double Weight { get; set; }
        public ICollection<IModifier> Modifiers { get; }

        public LootModifierSet(string name, int weight)
        {
            Name = name;
            Weight = weight;
            Modifiers = new List<IModifier>();
        }

        public void ModifyItem(object item)
        {
            if (Modifiers.Count == 0)
                return;

            foreach (var modifier in Modifiers)
                modifier.Apply(item);
        }

        public IModifierSet Add(IModifier modifier)
        {
            Modifiers.Add(modifier);
            return this;
        }

        public IModifierSet Remove(IModifier modifier)
        {
            Modifiers.Remove(modifier);
            return this;
        }

        public override string ToString()
        {
            return $"Name: {Name}, Weight: {Weight}, Modifier Count: {Modifiers.Count}";
        }
    }
}
