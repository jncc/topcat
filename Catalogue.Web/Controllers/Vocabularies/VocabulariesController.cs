using System;
using System.Collections.Generic;
using System.Web.Http;
using Catalogue.Data.Write;
using Catalogue.Gemini.Model;
using Raven.Client.Documents.Session;

namespace Catalogue.Web.Controllers.Vocabularies
{
    public class VocabulariesController : ApiController
    {
        private IVocabularyService service;
        private readonly IDocumentSession db;

        public VocabulariesController(IVocabularyService service, IDocumentSession db)
        {
            this.service = service;
            this.db = db;
        }

        public Vocabulary Get(String id)
        {
            if (id == "0")
            {
                return new Vocabulary
                    {
                        PublicationDate = DateTime.Now.ToString("MM-yyyy"),
                        Publishable = true,
                        Keywords = new List<VocabularyKeyword>()
                    };
            }

            return db.Load<Vocabulary>(id);
        }

        public VocabularyServiceResult Post([FromBody] Vocabulary vocab)
        {
            var result =  service.Insert(vocab);

            if (result.Success) db.SaveChanges();

            return result;
        }

        public VocabularyServiceResult Put(string id, [FromBody] Vocabulary vocab)
        {
            var result =  service.Update(vocab);

            if (result.Vocab.Id != id) throw new Exception("The id of the vocabulary does not match that supplied to the put method");

            if (result.Success) db.SaveChanges();

            return result;
        }
    }
}