using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Catalogue.Gemini.Model;

namespace Catalogue.Web.Admin.Vocabularies
{
    public class VocabulariesController : ApiController
    {
        private IVocabulariesService service;

        public VocabulariesController(IVocabulariesService service)
        {
            this.service = service;
        }

        public ICollection<Vocabulary> Get(String q)
        {
            return service.Read(q);
        }
    }
}