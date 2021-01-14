namespace Lorule.Client.Base.Types
{
    public class Tile
    { 
        public const int 
            WIDTH = 57, 
            HEIGHT = 28;

        public string Name { get; set; }
        public byte[] Data { get; set; }

        public override string ToString() => Name;
    }
}