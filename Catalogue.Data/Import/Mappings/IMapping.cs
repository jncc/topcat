using System.Collections.Generic;
using Catalogue.Gemini.Model;
using CsvHelper.Configuration;

namespace Catalogue.Data.Import.Mappings
{
    public interface IMapping
    {
        IEnumerable<Vocabulary> RequiredVocabularies { get; }
        void Apply(CsvConfiguration config);
    }
}
