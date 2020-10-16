#region

using System.Collections.Specialized;

#endregion

namespace Darkages.Types
{
    public class MetafileNode
    {
        public MetafileNode(string name, params string[] atoms)
        {
            Name = name;
            Atoms = new StringCollection();
            Atoms.AddRange(atoms);
        }

        public StringCollection Atoms { get; }
        public string Name { get; }
    }
}