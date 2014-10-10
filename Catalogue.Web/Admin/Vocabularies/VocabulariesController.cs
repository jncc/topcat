using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Catalogue.Web.Admin.Vocabularies
{
    public class VocabulariesController : ApiController
    {
        private IVocabulariesService service;

        public VocabulariesController(IVocabulariesService service)
        {
            this.service = service;
        }
    }
}