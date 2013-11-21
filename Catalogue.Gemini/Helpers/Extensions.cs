using System.Collections.Generic;
using System.Linq;
using Catalogue.Gemini.Model;

namespace Catalogue.Gemini.Helpers
{
    public static class Extensions
    {
        public static List<Keyword> ToKeywordList(this Dictionary<string, string> source)
        {
            return source
                .Select((pair => new Keyword { Vocab = pair.Key, Value = pair.Value }))
                .ToList();
        }

        public static List<Extent> ToExtentList(this Dictionary<string, string> source)
        {
            return source
                .Select((pair => new Extent { Authority = pair.Key, Value = pair.Value }))
                .ToList();
        }
    }
}
