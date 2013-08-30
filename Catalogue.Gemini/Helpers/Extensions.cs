using System.Collections.Generic;
using System.Linq;
using Catalogue.Gemini.Model;

namespace Catalogue.Gemini.Helpers
{
    public static class Extensions
    {
        public static List<Keyword> ToKeywordsList(this Dictionary<string, string> source)
        {
            return source
                .Select((pair => new Keyword { VocabularyIdentifier = pair.Key, Value = pair.Value }))
                .ToList();
        }
    }
}
