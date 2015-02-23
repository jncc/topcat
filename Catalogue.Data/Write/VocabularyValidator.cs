using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Gemini.Model;

namespace Catalogue.Data.Write
{
    public interface IVocabularyValidator
    {
        ValidationResult<Vocabulary> Validate(Vocabulary vocab);
    }

    public class VocabularyValidator : IVocabularyValidator
    {
        public ValidationResult<Vocabulary> Validate(Vocabulary vocab)
        {
            throw new NotImplementedException();
        }
    }

}
