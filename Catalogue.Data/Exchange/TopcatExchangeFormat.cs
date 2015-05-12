using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Import.Mappings;
using Catalogue.Gemini.Model;
using CsvHelper.Configuration;

namespace Catalogue.Data.Exchange
{
    public class TopcatExchangeFormat : IMapping
    {
        public IEnumerable<Vocabulary> RequiredVocabularies
        {
            get { throw new NotImplementedException(); }
        }

        public void Apply(CsvConfiguration config)
        {
            throw new NotImplementedException();
        }
    }
}
