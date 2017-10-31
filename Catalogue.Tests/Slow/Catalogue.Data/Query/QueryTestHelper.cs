using Catalogue.Data.Model;
using Catalogue.Data.Query;
using Catalogue.Gemini.Helpers;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using Catalogue.Utilities.Collections;
using static Catalogue.Data.Query.SortOptions;

namespace Catalogue.Tests.Slow.Catalogue.Data.Query
{
    public static class QueryTestHelper
    {
        public static RecordQueryInputModel EmptySearchInput()
        {
            return new RecordQueryInputModel
            {
                Q = "",
                F = null,
                P = 0,
                N = 25,
                O = MostRelevant
            };
        }

        public static Record SimpleRecord()
        {
            return new Record
            {
                Path = @"X:\some\path",
                Gemini = Library.Blank().With(m =>
                {
                    m.Title = "Some title";
                    m.Keywords = new StringPairList
                        {
                            { "http://vocab.jncc.gov.uk/jncc-domain", "Example Domain" },
                            { "http://vocab.jncc.gov.uk/jncc-category", "Example Category" },
                        }
                        .ToKeywordList();
                }),
            };
        }
    }
}
