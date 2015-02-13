using System;
using System.Web.Configuration;
using System.Web.Http;
using Catalogue.Data.Import;
using Catalogue.Data.Import.Mappings;
using Catalogue.Data.Write;
using Raven.Client;

namespace Catalogue.Web.Controllers.Ingest
{
    /*Slightly weird controller, should only be used once in live system, when importing the data*/

    public class Ingest
    {
        public int Id { get; set; }
        public bool SkipBadRecords { get; set; }
        public bool ImportKeywords { get; set; }
        public string FileName { get; set; }
    }

    public class IngestSettings
    {
        public string ImportFolderPath { get; set; }
    }

    public class IngestResult
    {
        public bool Success { get; set; }
        public string Exception { get; set; }
    }


    public class IngestController : ApiController
    {
        private IDocumentSession db;

        private string ImportFolderPath {
            get { return WebConfigurationManager.AppSettings["ImportFolderPath"]; }
        }

        public IngestController(IDocumentSession db)
        {
            this.db = db;
        }

        public IngestResult Post([FromBody] Ingest ingest)
        {
            if (ingest.Id == 0) return RunImport<TopcatMapping>(ingest);
            if (ingest.Id == 0) return RunImport<ActivitiesMapping>(ingest);
            if (ingest.Id == 1) return RunImport<MeshMapping>(ingest);
            if (ingest.Id == 2) return RunImport<PubCatMapping>(ingest);

            throw new ArgumentException("Invalid import id");
        }

        private IngestResult RunImport<T>(Ingest ingest) where T : IMapping, new()
        {
            var importer = Importer.CreateImporter<T>(db);
            importer.ImportKeywords = ingest.ImportKeywords;
            importer.SkipBadRecords = ingest.SkipBadRecords;

            importer.Import(ImportFolderPath + ingest.FileName);
            db.SaveChanges();


            return new IngestResult
            {
                Success = true
            };

        }

        public IngestSettings Get()
        {
            return new IngestSettings
                {
                    ImportFolderPath = this.ImportFolderPath
                };
        }

    }
}