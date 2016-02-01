using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalogue.Data.Import
{
    /// <summary>
    /// Thrown when an import fails.
    /// </summary>
    public class ImportException : Exception
    {
        public ImportException() { }
        public ImportException(string message) : base(message) { }
        public ImportException(string message, Exception innerException) : base(message, innerException) { }
    }
}
