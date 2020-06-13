#region

using System;
using System.Text.RegularExpressions;

#endregion

namespace Darkages.Systems.Loot.Modifiers
{
    [Serializable]
    public class StringModifier : BaseModifier
    {
        public StringModifier()
        {
        }

        public StringModifier(string propertyName, string newValue)
            : base(propertyName)
        {
            NewValue = newValue;
        }

        public string NewValue { get; set; }

        public override void Apply(object itemToModify)
        {
            var value = GetValue<string>(itemToModify);

            SetValue(itemToModify, Regex.Replace(NewValue, @"\{(.*?)\}", value));
        }
    }
}