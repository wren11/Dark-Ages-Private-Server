#region

using System;
using System.Collections.Generic;
using System.Reflection;

#endregion

namespace Darkages.Scripting
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ScriptAttribute : Attribute
    {
        public ScriptAttribute(string name, string author = "", string description = "")
        {
            Name = name;
            Author = author;
        }

        public string Name { get; }
        public string Author { get; }
    }

    public static class ScriptManager
    {
        private static readonly Dictionary<string, Type> scripts = new Dictionary<string, Type>();

        static ScriptManager()
        {
            var assembly = Assembly.GetExecutingAssembly();

            if (assembly == null)
                return;
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

                scripts.Add(attribute.Name, type);
            }
        }

        public static TScript LoadEach<TScript>(string name, params object[] args)
            where TScript : class
        {
            if (string.IsNullOrEmpty(name))
                return null;

            Type script;

            if (scripts.TryGetValue(name, out script))
            {
                var instance = Activator.CreateInstance(script, args);
                return instance as TScript;
            }

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

                Type script;

                if (scripts.TryGetValue(name, out script))
                {
                    var instance = Activator.CreateInstance(script, args);
                    data[name] = instance as TScript;
                }
            }

            if (data.Count == 2)
            {
            }

            return data;
        }
    }
}