namespace Darkages.Systems.Loot.Interfaces
{
    /// <summary>
    /// An interface for weighable items.
    /// </summary>
    public interface IWeighable
    {
        /// <summary>
        /// The weight of this item.
        /// </summary>
        double Weight { get; set; }
    }
}
