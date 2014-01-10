using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalogue.Gemini.DataFormats
{
    public static class DataFormatQueries
    {
        public static DataFormatInfo GetDataFormatInfo(string value)
        {
            // there must be a nicer way of writing this!

            // get the parent group of the format, if it's a known format, or the default "Other" group if not
            var theGroup = (from g in DataFormats.Known
                            from f in g.Formats
                            where f.Name == value
                            select g).FirstOrDefault() ?? DataFormats.Known.Single(g => g.Name == "Other");

            var theFormat = (from f in theGroup.Formats
                             where f.Name == value
                             select f).FirstOrDefault();

            return new DataFormatInfo
                {
                    Group = theGroup.Name,
                    Glyph = theGroup.Glyph,
                    Name = theFormat != null ? theFormat.Name : null,
                    Code = theFormat != null ? theFormat.Code : null
                };
        }

    }

    /// <summary>
    /// A shape for info about a data format along with its group.
    /// </summary>
    public class DataFormatInfo
    {
        public string Group { get; set; }
        public string Glyph { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
