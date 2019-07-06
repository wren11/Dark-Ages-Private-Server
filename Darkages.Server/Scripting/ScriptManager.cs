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
            {
                var instance = Activator.CreateInstance(script, args) as TScript;
                return instance;
            }

            return null;
        }
    }
}