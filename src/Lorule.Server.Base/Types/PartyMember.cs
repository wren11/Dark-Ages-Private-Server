namespace Darkages
{
    public class PartyMember
    {
        public PartyMember(Aisling _aisling)
        {
            Aisling = _aisling;
        }

        public Aisling Aisling { get; set; }
        public bool Leader { get; set; }
    }
}