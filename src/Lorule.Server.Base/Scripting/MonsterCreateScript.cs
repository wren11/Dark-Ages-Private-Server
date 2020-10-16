using Darkages.Types;

namespace Darkages.Scripting
{
    public abstract class MonsterCreateScript
    {
        public abstract Monster Create(MonsterTemplate template, Area map);
    }
}
