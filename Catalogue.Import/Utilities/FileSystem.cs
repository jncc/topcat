using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalogue.Import.Utilities
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
