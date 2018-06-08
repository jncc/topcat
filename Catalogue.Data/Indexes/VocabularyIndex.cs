using System.Linq;
using Catalogue.Data.Analyzers;
using Catalogue.Gemini.Model;
using Raven.Client.Documents.Indexes;

namespace Catalogue.Data.Indexes
{
    public class VocabularyIndex : AbstractIndexCreationTask<Vocabulary, Indexes.VocabularyIndex.Result>
    {
        public class Result
        {
            public string Vocab { get; set; }
            public string Name { get; set; }
            public string VocabN { get; set; }
        }

        public VocabularyIndex()
        {

            Map = vocabularies => from v in vocabularies
                                 select new
                                 {
                                     Vocab = v.Id,
                                     Name = v.Name,
                                     VocabN = v.Id

                                 };

            Reduce = xs => from x in xs
                           group x by x.Vocab into g
                           select new
                           {
                               Vocab = g.First().Vocab,
                               Name = g.First().Name,
                               VocabN = g.First().Vocab
                           };

            Stores.Add(x => x.Vocab, FieldStorage.Yes);
            Stores.Add(x => x.Name, FieldStorage.Yes);
            Analyze(x => x.VocabN, typeof(CustomKeywordAnalyzer).AssemblyQualifiedName); 
          
        }
    }
}
