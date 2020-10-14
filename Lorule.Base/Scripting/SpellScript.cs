#region

using Darkages.Network.Object;
using Darkages.Types;
using Newtonsoft.Json;

#endregion

namespace Darkages.Scripting
{
    public abstract class SpellScript : ObjectManager
    {
        public SpellScript(Spell spell)
        {
            Spell = spell;
        }

        [JsonIgnore] public string Arguments { get; set; }
        public bool IsScriptDefault { get; set; }
        public Spell Spell { get; set; }

        public virtual void OnActivated(Sprite sprite)
        {
        }

        public abstract void OnFailed(Sprite sprite, Sprite target);

        public virtual void OnSelectionToggle(Sprite sprite)
        {
        }

        public abstract void OnSuccess(Sprite sprite, Sprite target);

        public virtual void OnTriggeredBy(Sprite sprite, Sprite target)
        {
        }

        public abstract void OnUse(Sprite sprite, Sprite target);
    }
}