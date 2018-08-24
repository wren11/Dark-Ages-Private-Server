using Darkages.Types;
using System;

namespace Darkages.Scripting
{
    public abstract class WeaponScript
    {
        public WeaponScript(Item item)
        {
            Item = item;
        }

        public Item Item { get; set; }
        public abstract void OnUse(Sprite sprite, Action<int> cb = null);
    }
}
