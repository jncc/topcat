using System;
using System.Collections.Generic;
using System.Web.Http;
using Catalogue.Web.Admin.Keywords;
using Catalogue.Web.Admin.Vocabularies;

namespace Catalogue.Web.Controllers.Vocabularies
{
    public class VocabulariesController : ApiController
    {
        private readonly IVocabulariesService _service;

        public VocabulariesController(IVocabulariesService service)
        {
            _service = service;
        }

        public ICollection<string> Get(String q)
        {
            if (q == "all")
            {
                return _service.ReadAll();
            }

            return _service.Read(q);
        }
    }
}