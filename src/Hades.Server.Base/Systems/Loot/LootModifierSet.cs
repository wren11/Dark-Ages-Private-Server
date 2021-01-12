#region

using System.Collections.Generic;
using Darkages.Systems.Loot.Interfaces;
using Darkages.Types;

#endregion

namespace Darkages.Systems.Loot
{
    public class LootModifierSet : Template, IModifierSet
    {
        public LootModifierSet(string name, int weight)
        {
            Name = name;
            Weight = weight;
            Modifiers = new List<IModifier>();
        }

        public IList<IModifier> Modifiers { get; }
        public double Weight { get; set; }

        public IModifierSet Add(IModifier modifier)
        {
            Modifiers.Add(modifier);
            return this;
        }

        public override string[] GetMetaData()
        {
            return new[]
            {
                ""
            };
        }

        public void ModifyItem(object item)
        {
            if (Modifiers.Count == 0)
                return;

            foreach (var modifier in Modifiers)
                modifier.Apply(item);
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