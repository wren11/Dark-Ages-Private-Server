#region


#endregion

using Darkages.Network.Object;

namespace Darkages.Types
{
    public abstract class Template : ObjectManager
    {
        public string Description { get; set; }
        public string Group { get; set; }
        public string Name { get; set; }

        public abstract string[] GetMetaData();
    }
}