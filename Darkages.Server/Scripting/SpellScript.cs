///************************************************************************
//Project Lorule: A Dark Ages Server (http://darkages.creatorlink.net/index/)
//Copyright(C) 2018 TrippyInc Pty Ltd
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
//*************************************************************************/

using Darkages.Network.Object;
using Darkages.Types;
using Newtonsoft.Json;

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