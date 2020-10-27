using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lorule.Client.Base.Dat
{
    public interface IArchive
    {
        Task Load(string archiveName, bool save = false, string root = "Archives", string outputDirectory = "");
        ArchivedItem Get(string name, string archiveName);
        IEnumerable<ArchivedItem> SearchArchive(string extension, string stringPattern, string archiveName);
        void PackArchive(string unpackedDirectory, string outputFileName);
    }
}