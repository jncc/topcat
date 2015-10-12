using System;
using System.Collections.Generic;

namespace Catalogue.Gemini.Model
{
    public class Vocabulary
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PublicationDate { get; set; }

        public bool Publishable { get; set; } // is this a vocabulary of publishable keywords?
        public bool Controlled { get; set; }
     
        public List<VocabularyKeyword> Keywords { get; set; }
    }

    public class VocabularyKeyword
    {
        public string Value { get; set; }
        public string Description { get; set; }
    }
}
