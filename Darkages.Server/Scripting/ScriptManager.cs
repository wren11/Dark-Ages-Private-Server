using System;
using System.Collections.Generic;
using System.Reflection;

namespace Darkages.Scripting
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ScriptAttribute : Attribute
    {
        public ScriptAttribute(string name, string author = "")
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

        public static TScript Load<TScript>(string name, params object[] args)
            where TScript : class
        {
            if (string.IsNullOrEmpty(name))
                return null;

            Type script;

            if (scripts.TryGetValue(name, out script))
                return Activator.CreateInstance(script, args) as TScript;

            return null;
        }
    }
}