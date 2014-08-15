using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Web;
using Catalogue.Gemini.Model;

namespace Catalogue.Web.Admin.Keywords
{ public interface IKeywordsService
{
        ICollection<Keyword> ReadAll();
        ICollection<Keyword> ReadByValue(string value);
        ICollection<Keyword> ReadByVocab(string vocab);
        ICollection<Keyword> ReadByValueAndVocab(string value, string vocab);

    }

    public class KeywordsService : IKeywordsService
    {
        private readonly IKeywordsRepository _keywordsRepository;

        public KeywordsService(IKeywordsRepository keywordsRepository)
        {
            _keywordsRepository = keywordsRepository;
        }

        public ICollection<Keyword> ReadAll()
        {
            return _keywordsRepository.ReadAll();
        }

        public ICollection<Keyword> ReadByValue(string value)
        {
            return _keywordsRepository.ReadByValue(value);
        }

        public ICollection<Keyword> ReadByVocab(string vocab)
        {
            return _keywordsRepository.ReadByVocab(vocab);
        }

        public ICollection<Keyword> ReadByValueAndVocab(string value, string vocab)
        {
            // none of the above
            return _keywordsRepository.Read(value, vocab);
        }
    }

    
}