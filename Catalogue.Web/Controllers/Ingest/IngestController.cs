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

    public class Import
    {
        public int Id { get; set; }
        public bool SkipBadRecords { get; set; }
        public string FileName { get; set; }
    }

    public class ImportSettings
    {
        public string ImportFolderPath { get; set; }
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

        public Boolean Post([FromBody] Import import)
        {
            if (import.Id == 0) RunImport<ActivitiesMapping>(import);
            if (import.Id == 1) RunImport<MeshMapping>(import);
            if (import.Id == 2) RunImport<PubCatMapper>(import);

            return true;
        }

        private void RunImport<T>(Import import) where T : IMapping, new()
        {
            var vocabService = new VocabularyService(db);
            var importer = new Importer<T>(new FileSystem(), new RecordService(db,new RecordValidator(vocabService)), vocabService);
            importer.Import(ImportFolderPath + import.FileName);
            db.SaveChanges();
        }

        public ImportSettings Get()
        {
            return new ImportSettings
                {
                    ImportFolderPath = this.ImportFolderPath
                };
        }

    }
}