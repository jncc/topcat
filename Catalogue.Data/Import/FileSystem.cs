using System.IO;

namespace Catalogue.Data.Import
{
    public interface IFileSystem
    {
        TextReader OpenReader(string path);
    }

    public class FileSystem : IFileSystem
    {
        public TextReader OpenReader(string path)
        {
            return new StreamReader(path);
        }
    }
}
