#region

using System;
using System.IO;
using Darkages.Compression;

#endregion

namespace Darkages.Types
{
    public class MetafileManager
    {
        private static readonly MetafileCollection metafiles;

        static MetafileManager()
        {
            var files = Directory.GetFiles(Path.Combine(ServerContextBase.StoragePath, "metafile"));
            metafiles = new MetafileCollection(files.Length);

            foreach (var file in files)
                metafiles.Add(
                    CompressableObject.Load<Metafile>(file));
        }

        public static Metafile GetMetafile(string name)
        {
            return metafiles.Find(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public static MetafileCollection GetMetafiles()
        {
            return metafiles;
        }
    }
}