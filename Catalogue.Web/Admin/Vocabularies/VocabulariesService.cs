using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Catalogue.Gemini.Model;

namespace Catalogue.Web.Admin.Vocabularies
{
    public interface IVocabulariesService
    {
        ICollection<Vocabulary> Read(string s);
    }

    public class VocabulariesService : IVocabulariesService
    {
        private readonly IVocabulariesRepository _repo;

        public VocabulariesService(IVocabulariesRepository repo)
        {
            _repo = repo;
        }

        public ICollection<Vocabulary> Read(string vocab)
        {
            return _repo.Read(vocab);
        }


    }
}