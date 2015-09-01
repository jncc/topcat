using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;

namespace Catalogue.Data.Import
{
    /// <summary>
    /// The Csv Helper library has got a nasty API. This unifies it a bit for our purposes.
    /// </summary>
    public static class CsvHelperHelpers
    {
        public static CsvPropertyMap Field(this CsvPropertyMap source, string name)
        {
            return source.Name(name);
        }

        public static CsvPropertyMap Field<T>(this CsvPropertyMap source, string name, Func<string, T> modifier)
        {
            return source.ConvertUsing(row =>
            {
                string value = row.GetField(name);
                return modifier(value);
            });
        }

        public static CsvPropertyMap Value<T>(this CsvPropertyMap source, T value)
        {
            return source.ConvertUsing(row => value);
        }
    }
}
