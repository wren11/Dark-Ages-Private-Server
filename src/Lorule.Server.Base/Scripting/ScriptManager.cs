#region

using System;
using System.Collections.Generic;
using System.Reflection;

#endregion

namespace Darkages.Scripting
{
    public static class ScriptManager
    {
        private static readonly Dictionary<string, Type> Scripts = new Dictionary<string, Type>();

        static ScriptManager()
        {
            var assembly = Assembly.GetExecutingAssembly();

            foreach (var type in assembly.GetTypes())
            {
                ScriptAttribute attribute = null;

                foreach (ScriptAttribute attr in type.GetCustomAttributes(typeof(ScriptAttribute), false))
                {
                    attribute = attr;
                    break;
                }

                if (attribute == null)
                    continue;

                Scripts.Add(attribute.Name, type);
            }
        }

        public static object GetScript(string name)
        {
            if (!string.IsNullOrEmpty(name) && Scripts.ContainsKey(name))
                return Scripts[name];

            return null;
        }

        public static Dictionary<string, TScript> Load<TScript>(string values, params object[] args)
            where TScript : class
        {
            if (values == null)
                return null;

            var names = values.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);

            var data = new Dictionary<string, TScript>();

            foreach (var name in names)
            {
                if (string.IsNullOrEmpty(name))
                    continue;

                if (!Scripts.TryGetValue(name, out var script))
                    continue;

                var instance = Activator.CreateInstance(script, args);

                data[name] = instance as TScript;
            }

            if (data.Count == 2)
            {
            }

            return data;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ScriptAttribute : Attribute
    {
        public ScriptAttribute(string name, string author = "", string description = "")
        {
            Description = description;
            Name = name;
            Author = author;
        }

        public string Author { get; }
        public string Description { get; }
        public string Name { get; }
    }
}