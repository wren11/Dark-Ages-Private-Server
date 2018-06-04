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
using System.IO;
using System.Reflection;
using System.Runtime.Caching;
using Attribute = System.Attribute;

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
            var SCRIPTS = ServerContext.StoragePath + "\\Scripts";
            var TOTALSCRIPTS = Directory.GetFiles(SCRIPTS, "*.cs", SearchOption.AllDirectories);
            var SCRIPTSPROCESSED = 0;

            Console.WriteLine("[Lorule] Loading Game Scripts...");
            Console.CursorLeft = 0;
            ObjectCache cache = MemoryCache.Default;

            foreach (var script in TOTALSCRIPTS)
            {
                var scriptKey = Path.GetFileNameWithoutExtension(script);
                var scriptContents = cache[scriptKey] as string;

                if (scriptContents == null)
                {
                    CacheItemPolicy policy = new CacheItemPolicy();
                    policy.ChangeMonitors.Add(new HostFileChangeMonitor(new string[] { Path.GetFullPath(script) }));

                    scriptContents = File.ReadAllText(Path.GetFullPath(script));
                    cache.Set("source", scriptContents, policy);
                }

                var assembly = GenerateScript(Path.GetFileNameWithoutExtension(script), scriptContents);
                var types = assembly.GetTypes();

                foreach (var type in types)
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


                drawTextProgressBar(string.Format("Processing Script:'{0}'", Path.GetFileNameWithoutExtension(script)), SCRIPTSPROCESSED++, TOTALSCRIPTS.Length);
                Console.CursorLeft = 0;
            }

            Console.WriteLine("");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.CursorTop = Console.CursorTop -= 1;
            Console.WriteLine("[Lorule] Servers Online.".PadRight(100));
            Console.WriteLine("--------------------------------------------------------------------------------");
        }



        public static Assembly GenerateScript(string file, string source)
        {
            var provider_opts = new Dictionary<string, string>
            {
                { "CompilerVersion","v4.0"}
            };

            var provider = new Microsoft.CSharp.CSharpCodeProvider(provider_opts);
            var compiler_params = new System.CodeDom.Compiler.CompilerParameters();

            compiler_params.GenerateInMemory = true;
            compiler_params.GenerateExecutable = false;
            compiler_params.ReferencedAssemblies.Add(typeof(ServerContext).Assembly.Location);
            compiler_params.ReferencedAssemblies.Add(typeof(object).Assembly.Location);
            compiler_params.ReferencedAssemblies.Add("System.dll");
            compiler_params.ReferencedAssemblies.Add("System.IO.dll");
            compiler_params.ReferencedAssemblies.Add("System.Linq.dll");
            compiler_params.ReferencedAssemblies.Add("System.Core.dll");
            compiler_params.ReferencedAssemblies.Add("System.Collections.dll");

            source = source.Replace("?.", ".");
            
            var results = provider.CompileAssemblyFromSource(compiler_params, source);
            var type = results.CompiledAssembly;

            if (results.Errors.Count > 0)
            {

            }

            return type;
        }

        //shamelessly copy pasted from : https://stackoverflow.com/questions/24918768/progress-bar-in-console-application
        public static void drawTextProgressBar(string stepDescription, int progress, int total)
        {
            int totalChunks = 30;

            //draw empty progress bar
            Console.CursorLeft = 0;
            Console.Write(""); //start
            Console.CursorLeft = totalChunks + 1;
            Console.Write(""); //end
            Console.CursorLeft = 1;

            double pctComplete = Convert.ToDouble(progress) / total;
            int numChunksComplete = Convert.ToInt16(totalChunks * pctComplete);

            //draw completed chunks
            Console.BackgroundColor = ConsoleColor.Green;
            Console.Write("".PadRight(numChunksComplete));

            //draw incomplete chunks
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.Write("".PadRight(totalChunks - numChunksComplete));

            //draw totals
            Console.CursorLeft = totalChunks + 5;
            Console.BackgroundColor = ConsoleColor.Black;

            string output = stepDescription;
            Console.Write(output.PadRight(50)); 
        }

        public static TScript Load<TScript>(string name, params object[] args)
            where TScript : class
        {
            if (string.IsNullOrEmpty(name))
                return null;

            Type script;

            if (scripts.TryGetValue(name, out script))
            {
                return Activator.CreateInstance(script, args) as TScript;
            }

            return null;
        }
    }
}
