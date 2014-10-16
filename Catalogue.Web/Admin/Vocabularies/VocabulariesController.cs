using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Catalogue.Gemini.Model;
using Catalogue.Web.Admin.Vocabularies;

namespace Catalogue.Web.Controllers.Vocabularies
{
    public class VocabulariesController : ApiController
    {
        private IVocabulariesService service;

        public VocabulariesController(IVocabulariesService service)
        {
            this.service = service;
        }

        public Vocabulary Get(String id)
        {
            //Handles expected call with q = null when initialising editor for new vocab.
            if (String.IsNullOrWhiteSpace(id))
            {
                return new Vocabulary
                    {
                        PublicationDate = DateTime.Now.ToString("MM-yyyy"),
                        Publishable = true
                    };
            }

            return service.Read(id);
        }
    }
}