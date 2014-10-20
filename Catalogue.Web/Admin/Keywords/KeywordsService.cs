using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Web;
using Catalogue.Gemini.Model;

namespace Catalogue.Web.Admin.Keywords
{ public interface IKeywordsService
{
        ICollection<MetadataKeyword> ReadAll();
        ICollection<MetadataKeyword> ReadByValue(string value);
        ICollection<MetadataKeyword> ReadByVocab(string vocab);
        ICollection<MetadataKeyword> ReadByValueAndVocab(string value, string vocab);

    }

    public class KeywordsService : IKeywordsService
    {
        private readonly IKeywordsRepository _keywordsRepository;

        public KeywordsService(IKeywordsRepository keywordsRepository)
        {
            _keywordsRepository = keywordsRepository;
        }

        public ICollection<MetadataKeyword> ReadAll()
        {
            return _keywordsRepository.ReadAll();
        }

        public ICollection<MetadataKeyword> ReadByValue(string value)
        {
            return _keywordsRepository.ReadByValue(value);
        }

        public ICollection<MetadataKeyword> ReadByVocab(string vocab)
        {
            return _keywordsRepository.ReadByVocab(vocab);
        }

        public ICollection<MetadataKeyword> ReadByValueAndVocab(string value, string vocab)
        {
            // none of the above
            return _keywordsRepository.Read(value, vocab);
        }
    }

    
}