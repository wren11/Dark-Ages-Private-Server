using CommandLine;
using Lorule.Client.Base.Dat;
using Lorule.Content.Editor.Dat;
using Lorule.Content.Editor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack;
using System;
using System.Collections.Generic;
using Lorule.Editor;

namespace Hades.Utils.TileAMerge
{
    class MergeOptions
    {
        [Option('a', "ArchiveA", Required = true)]
        public string ArchiveA { get; set; }

        [Option('b', "ArchiveB", Required = true)]
        public string ArchiveB { get; set; }

        [Option('t', "Target", Required = true)]
        public string Target { get; set; }

        [Option('o', "Output", Required = true)]
        public string Output { get; set; }
    }

    class Program
    {
        //Example Command Line : TileAMerge.exe --a "seo\seo1.dat" --b "seo\seo2.dat" --t TILEA.BMP --o NEWTILEA.BMP"

        static void Main(string[] args)
        {
            void RunMerger(ServiceProvider serviceProvider1)
            {
                var merger = serviceProvider1?.GetService<TileMerger>();
                Parser.Default.ParseArguments<MergeOptions>(args)
                    .WithNotParsed(HandleParseError)
                    .WithParsed(async o =>
                    {
                        if (merger != null)
                            await merger.Combine(o.ArchiveA, o.ArchiveB, o.Target, o.Output);
                    });
            }

            var builder = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("LoruleConfig.json");

            var config = builder.Build();
            var editorSettings = config.GetSection("Editor").Get<EditorOptions>();
            using var serviceProvider = new ServiceCollection()
                .AddSingleton<IArchive, Archive>(_ => new Archive(editorSettings.Location))
                .AddSingleton<IPaletteCollection, PaletteCollection>()
                .AddSingleton<TileMerger>()
                .BuildServiceProvider();

            RunMerger(serviceProvider);
        }

        private static void HandleParseError(IEnumerable<Error> obj)
        {
            Console.WriteLine("Error, Not able to parse input arguments.");
        }
    }
}
