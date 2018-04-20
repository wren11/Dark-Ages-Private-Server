namespace Darkages.Systems.Loot.Interfaces
{
    public interface IModifier
    {
        /// <summary>
        /// Apply this modifier to an item
        /// </summary>
        void Apply(object item);
    }
}
