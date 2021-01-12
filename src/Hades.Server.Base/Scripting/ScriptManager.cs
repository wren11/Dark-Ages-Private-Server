#region

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

#endregion

namespace Darkages.Scripting
{
    public static class ScriptManager
    {
        public static Dictionary<string, Type> Scripts = new Dictionary<string, Type>();

        public static void LoadAndCacheScripts()
        {
            var dotNetCoreDir = Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location);
            var scriptFiles = Directory.GetFiles(Path.Combine(ServerContext.StoragePath, @"scripts"), "*.cs", SearchOption.AllDirectories);
            var currentDir = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) ?? string.Empty;
            var assemblies = Directory.GetFiles(currentDir, "*.dll", SearchOption.TopDirectoryOnly);
            var metadataReferences = assemblies.Select(n => MetadataReference.CreateFromFile(n)).ToList();

            foreach (var dll in Directory.GetFiles(Environment.CurrentDirectory, "*.dll", SearchOption.AllDirectories))
            {
                if (dll.Contains("in"))
                    continue;

                metadataReferences.Add(MetadataReference.CreateFromFile(dll));
            }

            metadataReferences.Add(MetadataReference.CreateFromFile(typeof(List<>).GetTypeInfo().Assembly.Location));
            metadataReferences.Add(MetadataReference.CreateFromFile(typeof(Collection<>).GetTypeInfo().Assembly.Location));
            metadataReferences.Add(MetadataReference.CreateFromFile(typeof(Enumerable).GetTypeInfo().Assembly.Location));
            metadataReferences.Add(MetadataReference.CreateFromFile(typeof(Guid).GetTypeInfo().Assembly.Location));
            metadataReferences.Add(MetadataReference.CreateFromFile(typeof(DateTime).GetTypeInfo().Assembly.Location));
            metadataReferences.Add(MetadataReference.CreateFromFile(typeof(CSharpCompilation).GetTypeInfo().Assembly.Location));
            metadataReferences.Add(MetadataReference.CreateFromFile(typeof(ScriptAttribute).GetTypeInfo().Assembly.Location));
            metadataReferences.Add(MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location));
            metadataReferences.Add(MetadataReference.CreateFromFile(typeof(Enum).GetTypeInfo().Assembly.Location));
            metadataReferences.Add(MetadataReference.CreateFromFile(typeof(IEnumerable<>).GetTypeInfo().Assembly.Location));
            metadataReferences.Add(MetadataReference.CreateFromFile(typeof(File).GetTypeInfo().Assembly.Location));
            metadataReferences.Add(MetadataReference.CreateFromFile(Path.Combine(dotNetCoreDir, "System.Runtime.dll")));
            metadataReferences.Add(MetadataReference.CreateFromFile(Path.Combine(dotNetCoreDir, "System.Collections.dll")));
            metadataReferences.Add(MetadataReference.CreateFromFile(Path.Combine(dotNetCoreDir, "System.Linq.dll")));
            metadataReferences.Add(MetadataReference.CreateFromFile(Path.Combine(dotNetCoreDir, "System.IO.dll")));
            metadataReferences.Add(MetadataReference.CreateFromFile(Path.Combine(dotNetCoreDir, "System.dll")));
            metadataReferences.Add(MetadataReference.CreateFromFile(Path.Combine(dotNetCoreDir, "System.Collections.Concurrent.dll")));
            metadataReferences = metadataReferences.Distinct().ToList();

            var assemblyName = Path.GetRandomFileName();

            var compilation = CSharpCompilation.Create(
                assemblyName,
                scriptFiles.Select(n => CSharpSyntaxTree.ParseText(File.ReadAllText(n))),
                metadataReferences.ToArray(), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            ServerContext.Logger("Compiling all scripts...");

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        ServerContext.Logger("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    Assembly compiled_assembly = Assembly.Load(ms.ToArray());

                    LoadFromAssembly(compiled_assembly);
                }
            }

            ServerContext.Logger("Compiling all scripts... completed.");
        }

        public static void LoadFromAssembly(Assembly assembly)
        {
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