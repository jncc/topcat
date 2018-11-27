using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Gemini.Model;
using CsvHelper.Configuration;

namespace Catalogue.Data.Import
{
    /// <summary>
    /// The Csv Helper library has got a nasty API. This unifies it a bit for our purposes.
    /// </summary>
    public static class CsvHelperHelpers
    {
        public static MemberMap Field(this MemberMap source, string name)
        {
            return source.Name(name);
        }
    }
}
