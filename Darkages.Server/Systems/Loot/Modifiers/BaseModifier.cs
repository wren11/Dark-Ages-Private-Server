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
using Darkages.Systems.Loot.Interfaces;
using System;
using System.Reflection;

namespace Darkages.Systems.Loot.Modifiers
{
    [Serializable]
    public abstract class BaseModifier : IModifier
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public;

        public string PropertyName { get; set; }

        public abstract void Apply(object itemToModify);

        protected BaseModifier() { }
        protected BaseModifier(string propertyName)
        {
            PropertyName = propertyName;
        }

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

            return default(T);
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
