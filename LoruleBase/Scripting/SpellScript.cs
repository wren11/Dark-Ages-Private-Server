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

        public Spell Spell { get; set; }

        [JsonIgnore] public string Arguments { get; set; }

        public bool IsScriptDefault { get; set; }

        public abstract void OnUse(Sprite sprite, Sprite target);
        public abstract void OnFailed(Sprite sprite, Sprite target);
        public abstract void OnSuccess(Sprite sprite, Sprite target);

        public virtual void OnSelectionToggle(Sprite sprite)
        {
        }

        public virtual void OnActivated(Sprite sprite)
        {
        }

        public virtual void OnTriggeredBy(Sprite sprite, Sprite target)
        {
        }
    }
}