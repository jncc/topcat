using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Web;
using Catalogue.Gemini.Model;

namespace Catalogue.Web.Admin.Keywords
{ public interface IKeywordsService
    {
        List<Gemini.Model.Keyword> Read(String value = null, String vocab = null);
        /*Gemini.Model.Keyword Read();
        void Delete();*/

    }

    public class KeywordsService : IKeywordsService
    {
        private readonly IKeywordsRepository _keywordsRepository;

        public KeywordsService(IKeywordsRepository keywordsRepository)
        {
            _keywordsRepository = keywordsRepository;
        }

        public List<Keyword> ReadAll()
        {
            return _keywordsRepository.ReadAll();
        }

        public List<Keyword> Read(string value = null, string vocab = null)
        {
            if (String.IsNullOrEmpty(value) && String.IsNullOrEmpty(vocab))
            {
              return _keywordsRepository.ReadAll();
            }
            
            if (String.IsNullOrEmpty(value) && !String.IsNullOrEmpty(vocab))
            {
               return _keywordsRepository.ReadAllByVocab(vocab);
            }
            
            if (!String.IsNullOrEmpty(value) && String.IsNullOrEmpty(vocab))
            {
                return _keywordsRepository.ReadAllByValue(value);
            }
            // none of the above
            return _keywordsRepository.Read(value, vocab);
        }
    }

    
}