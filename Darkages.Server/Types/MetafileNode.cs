using System.Collections.Specialized;

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

        public string Name { get; }
        public StringCollection Atoms { get; }
    }
}