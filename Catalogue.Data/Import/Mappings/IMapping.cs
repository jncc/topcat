using System.Collections.Generic;
using Catalogue.Gemini.Model;
using CsvHelper.Configuration;

namespace Catalogue.Data.Import.Mappings
{
    public interface IWriterMapping
    {
        IEnumerable<Vocabulary> RequiredVocabularies { get; }
        void Apply(IWriterConfiguration config);
    }

    public interface IReaderMapping
    {
        IEnumerable<Vocabulary> RequiredVocabularies { get; }
        void Apply(IReaderConfiguration config);
    }
}
