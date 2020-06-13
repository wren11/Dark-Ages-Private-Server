#region

using Darkages.Network.Object;

#endregion

namespace Darkages.Types
{
    public abstract class Template : ObjectManager
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Group { get; set; }
    }
}