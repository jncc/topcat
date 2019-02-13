using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Catalogue.Data.Write;
using log4net;
using Raven.Client.Listeners;
using Raven.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using Raven.Client.Documents;

namespace Catalogue.Toolbox.Patch.DataPatchers
{
    public class PublishingPatch : IDataPatcher
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PublishingPatch));

        readonly IDocumentStore store;
        readonly List<RecordServiceResult> results = new List<RecordServiceResult>();

        public PublishingPatch(IDocumentStore store)
        {
            this.store = store;
        }

        public List<RecordServiceResult> Patch()
        {
            using (var db = store.OpenSession())
            {
                Logger.Info("Running publishing patch");

                var records = db
                    .Query<Record>()
                    .Skip(0)
                    .Take(5000)
                    .ToList();

                db.SaveChanges();
            }

            return results;
        }
    }

    public class PublishingConverter : IDocumentConversionListener
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PublishingConverter));

        public void BeforeConversionToDocument(string key, object entity, RavenJObject metadata)
        {
            
        }

        public void AfterConversionToDocument(string key, object entity, RavenJObject document, RavenJObject metadata)
        {
            
        }

        public void BeforeConversionToEntity(string key, RavenJObject document, RavenJObject metadata)
        {
            
        }

        public void AfterConversionToEntity(string key, RavenJObject document, RavenJObject metadata, object entity)
        {
            Record r = entity as Record;
            if (r == null)
                return;

            Logger.Info("Starting conversion of old fields to new");
            if (!string.IsNullOrWhiteSpace(document["Gemini"].Value<string>("ResourceLocator")))
            {
                Logger.Info("Migrating Gemini.ResourceLocator");

                if (r.Publication == null)
                {
                    r.Publication = new PublicationInfo();
                }

                if (r.Publication.Data == null)
                {
                    r.Publication.Data = new DataInfo();
                }

                if (r.Publication.Data.Resources == null)
                {
                    r.Publication.Data.Resources = new List<Resource>();
                }

                r.Publication.Data.Resources.Add(new Resource
                {
                    Name = document["Gemini"].Value<string>("ResourceLocator"),
                    Path = document["Gemini"].Value<string>("ResourceLocator")
                });
            }
        }
    }
}
