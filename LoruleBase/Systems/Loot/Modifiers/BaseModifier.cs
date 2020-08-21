#region

using System;
using System.Reflection;
using Darkages.Systems.Loot.Interfaces;

#endregion

namespace Darkages.Systems.Loot.Modifiers
{
    [Serializable]
    public abstract class BaseModifier : IModifier
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public;

        protected BaseModifier()
        {
        }

        protected BaseModifier(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; set; }

        public abstract void Apply(object itemToModify);

        protected T GetValue<T>(object objectInstance)
        {
            var property = objectInstance.GetType().GetProperty(PropertyName, Flags);

            if (property != null)
            {
                var value = property.GetValue(objectInstance);
                try
                {
                    return (T)value;
                }
                catch
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
            }

            var field = objectInstance.GetType().GetField(PropertyName, Flags);

            if (field != null)
            {
                var value = field.GetValue(objectInstance);
                try
                {
                    return (T)value;
                }
                catch
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
            }

            return default;
        }

        protected void SetValue(object objectInstance, object value)
        {
            var property = objectInstance.GetType().GetProperty(PropertyName, Flags);

            if (property != null)
            {
                try
                {
                    property.SetValue(objectInstance, value);
                }
                catch
                {
                    property.SetValue(objectInstance, Convert.ChangeType(value, property.PropertyType));
                }

                return;
            }

            var field = objectInstance.GetType().GetField(PropertyName, Flags);

            if (field == null)
                return;

            try
            {
                field.SetValue(objectInstance, value);
            }
            catch
            {
                field.SetValue(objectInstance, Convert.ChangeType(value, field.FieldType));
            }
        }
    }
}