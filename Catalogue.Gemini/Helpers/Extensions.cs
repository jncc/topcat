using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Collections;

namespace Catalogue.Gemini.Helpers
{
    public static class Extensions
    {
        public static List<MetadataKeyword> ToKeywordList(this StringPairList source)
        {
            return source
                .Select((pair => new MetadataKeyword { Vocab = pair.Item1, Value = pair.Item2 }))
                .ToList();
        }

        public static bool IsEqualTo(this List<MetadataKeyword> source, List<MetadataKeyword> other)
        {
            return source.Count == other.Count &&
                source.Zip(other, (a, b) => new { a, b })
                    .All(x => x.a.Vocab == x.b.Vocab && x.a.Value == x.b.Value);
        }

        public static List<Extent> ToExtentList(this StringPairList source)
        {
            return source
                .Select((pair => new Extent { Authority = pair.Item1, Value = pair.Item2 }))
                .ToList();
        }
    }
}
