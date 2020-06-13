namespace Darkages.Types
{
    public class MapKeyPair
    {
        public MapKeyPair(int _number, ushort _key)
        {
            Number = _number;
            Key = _key;
        }

        public int Number { get; set; }

        public ushort Key { get; set; }
    }
}