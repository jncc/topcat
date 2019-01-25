using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Raven;
using Raven.Client.Documents.Session;
using Catalogue.Data;

namespace Catalogue.Web.Controllers.Dumps
{
    public class DumpsController : ApiController
    {
        readonly IDocumentSession db;

        public DumpsController(IDocumentSession db)
        {
            this.db = db;
        }

        [HttpGet, Route("api/dumps/allkeywordsinrecords")]
        public List<MetadataKeyword> AllKeywordsInRecords()
        {
            var result = db.Query<RecordKeywordIndex.Result, RecordKeywordIndex>().Fetch(1024);
            
            return result.Select(k => new MetadataKeyword { Vocab = k.Vocab, Value = k.Value }).ToList();
        }

        [HttpGet, Route("api/dumps/recordswithgdbinpath")]
        public List<string> RecordsWithGdbInPath()
        {
            var result = db.Query<Record>().Fetch(1024);

            return result
                .Where(r => r.Path.Contains(".gdb"))
                .Select(r => r.Id.ToString())
                .ToList();
        }

        [HttpGet, Route("api/dumps/recordswithpublishinginfo")]
        public List<RecordWithPublicationInfoResultShape> RecordsWithPublishingInfo()
        {
            var results = db.Query<Record, RecordsWithOpenDataPublicationInfoIndex>().Fetch(1024);

            return results.Select(r => new RecordWithPublicationInfoResultShape
                {
                    Id = Helpers.RemoveCollection(r.Id),
                    Title = r.Gemini.Title,
                    MetadataDate = r.Gemini.MetadataDate,
                    PublicationInfo = r.Publication.Gov,
                }).ToList();
        }

        [HttpGet, Route("api/dumps/recordcountbyvocabandkeyword")]
        public List<RecordCountByVocabAndKeywordResultShape> RecordCountByVocabAndKeyword()
        {
            var results = db.Query<RecordCountForKeywordIndex.Result, RecordCountForKeywordIndex>().Fetch(5000); // but server maxpagesize is..?

            var q = from r in results
                    where r.KeywordVocab != ""
                    orderby r.KeywordVocab == "http://vocab.jncc.gov.uk/jncc-domain" || r.KeywordVocab == "http://vocab.jncc.gov.uk/jncc-category" descending 
                    group r by r.KeywordVocab into g
                    select new RecordCountByVocabAndKeywordResultShape
                    {
                        Vocab = g.Key,
                        KeywordCount = g.Count(),
                        RecordCount = g.Sum(x => x.RecordCount),
                        Keywords = from x in g
                                   where x.RecordCount > 1
                                   select new RecordCountByVocabAndKeywordResultShape.RecordCountByKeywordResultShape
                                   {
                                       Keyword = x.KeywordValue,
                                       RecordCount = x.RecordCount
                                   }
                    };

            return q.ToList();
        }

        public class RecordWithPublicationInfoResultShape
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public DateTime MetadataDate { get; set; }
            public GovPublicationInfo PublicationInfo { get; set; }

        }

        public class RecordCountByVocabAndKeywordResultShape
        {
            public string Vocab { get; set; }
            public int KeywordCount { get; set; }
            public int RecordCount { get; set; }
            public IEnumerable<RecordCountByKeywordResultShape> Keywords { get; set; }

            public class RecordCountByKeywordResultShape
            {
                public string Keyword { get; set; }
                public int RecordCount { get; set; }
            }
        }

        [HttpGet, Route("api/dumps/recordsnotpublishedsincelastupdated")]
        public List<RecordWithPublicationInfoResultShape> RecordsNotPublishedSinceLastUpdated()
        {
            var results = db.Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>()
                .Where(r => !r.PublishedSinceLastUpdated)
                .OfType<Record>()
                .Fetch(1024);

            return results.Select(r => new RecordWithPublicationInfoResultShape
            {
                Id = Helpers.RemoveCollection(r.Id),
                Title = r.Gemini.Title,
                MetadataDate = r.Gemini.MetadataDate,
                PublicationInfo = r.Publication.Gov,
            }).ToList();
        }

    }
}
