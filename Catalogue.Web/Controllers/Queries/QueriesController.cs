using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Catalogue.Data.Query;
using Raven.Client;

namespace Catalogue.Web.Controllers.Queries
{
    /// <summary>
    /// Ad-hoc queries for people.
    /// </summary>
    public class QueriesController : ApiController
    {
        IDocumentSession db;
        IRecordQueryer recordQueryer;

        public QueriesController(IDocumentSession db, IRecordQueryer recordQueryer)
        {
            this.db = db;
            this.recordQueryer = recordQueryer;
        }

        [HttpGet, Route("api/queries/SeabedHabitatMapsWithNoGui")]
        public List<string> SeabedHabitatMapsWithNoGui()
        {
            var query = new RecordQueryInputModel
                {
                    N = 1000,
                    K = new [] { "vocab.jncc.gov.uk/jncc-category/Seabed Habitat Maps" },
                };

            var records = recordQueryer.RecordQuery(query);

            var q = from r in records
                    where !(from k in r.Gemini.Keywords
                            where k.Vocab == "http://vocab.jncc.gov.uk/mesh-gui"
                            select k.Value).Any()
                    select r.Id.ToString();

            return q.ToList();
        }
    }
}