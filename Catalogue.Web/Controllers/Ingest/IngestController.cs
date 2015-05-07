using System;
using System.Web.Http;
using Catalogue.Data.Import;
using Catalogue.Data.Import.Mappings;
using Raven.Client;

namespace Catalogue.Web.Controllers.Ingest
{
    public class Model
    {
        public int Id { get; set; }
        public bool SkipBadRecords { get; set; }
        public string FileName { get; set; }
    }

    public class IngestResult
    {
        public bool Success { get; set; }
        public string Exception { get; set; }
    }

    public class IngestController : ApiController
    {
        private IDocumentSession db;

        public IngestController(IDocumentSession db)
        {
            this.db = db;
        }

        public IngestResult Post([FromBody] Model model)
        {
            if (model.Id == 0) return RunImport<TopcatMapping>(model);
            if (model.Id == 1) return RunImport<ActivitiesMapping>(model);
            if (model.Id == 2) return RunImport<MeshMapping>(model);
            if (model.Id == 3) return RunImport<PubCatMapping>(model);

            throw new ArgumentException("Invalid import id");
        }

        private IngestResult RunImport<T>(Model model) where T : IMapping, new()
        {
            var importer = Importer.CreateImporter<T>(db);
            importer.SkipBadRecords = model.SkipBadRecords;
            importer.Import(model.FileName);
            db.SaveChanges();

            return new IngestResult { Success = true };
        }
    }
}
