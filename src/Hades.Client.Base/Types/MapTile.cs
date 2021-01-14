namespace Lorule.Client.Base.Types
{
    public class MapTile
    {
        public readonly ushort Floor;
        public readonly ushort Left;
        public readonly ushort Right;

        public MapTile(ushort floor, ushort left, ushort right)
        {
            Floor = floor;
            Left  = left;
            Right = right;
        }
    }
}