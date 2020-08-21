#region

using Darkages.Compression;
using System;
using System.Collections.ObjectModel;
using System.IO;
using Darkages.IO;
using ServiceStack;

#endregion

namespace Darkages.Types
{
    public class MetafileManager
    {
        private static readonly MetafileCollection Metafiles;

        static MetafileManager()
        {
            var files = Directory.GetFiles(Path.Combine(ServerContextBase.StoragePath, "metafile"));
            Metafiles = new MetafileCollection(files.Length);

            //foreach (var file in files)
            //{
            //    var metaFile = CompressableObject.Load<Metafile>(file);

            //    Metafiles.Add(metaFile);
            //}

            CreateFromTemplates();
        }

        private static void CreateFromTemplates()
        {
            var i = 0;
            foreach (var batch in ServerContextBase.GlobalItemTemplateCache.BatchesOf(712))
            { 
                //var idx = i++ % ServerContextBase.GlobalItemTemplateCache.Count / 712;

                var metaFile = new Metafile {Name = $"ItemInfo{i}", Nodes = new Collection<MetafileNode>()};

                foreach (var (k, template) in batch)
                {
                    if (template.Gender == 0)
                    {
                        continue;
                    }

                    var meta = template.GetMetaData();
                    metaFile.Nodes.Add(new MetafileNode(k, meta));
                }

                using (var stream = new MemoryStream())
                {
                    metaFile.Save(stream);
                    metaFile.InflatedData = stream.ToArray();
                }

                metaFile.Compress();
                metaFile.Hash = Crc32Provider.ComputeChecksum(metaFile.InflatedData);


                Metafiles.Add(metaFile);

                i++;
            }
        }

        public static Metafile GetMetafile(string name)
        {
            return Metafiles.Find(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public static MetafileCollection GetMetafiles()
        {
            return Metafiles;
        }
    }
}