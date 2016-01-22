using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Import.Mappings;
using Raven.Client;

namespace Catalogue.Robot.Importing
{
    public class ImportHandler
    {
        readonly IDocumentSession db;

        public ImportHandler(IDocumentSession db)
        {
            this.db = db;
        }

        public void Import(ImportOptions options)
        {

            var importer = global::Catalogue.Data.Import.Importer.CreateImporter<MarineRecorderMapping>(db);
            importer.SkipBadRecords = options.SkipBad;
            importer.Import(options.File); //.Trim('"')  needed, or hopefully will the CommandLineParser strip out quotes for me?
            db.SaveChanges();

            int successes = importer.Results.Count(r => r.Success);
            Console.WriteLine(successes);
        }

//        IMapping GetMapping(string arg)
//        {
////            if (arg == "topcat") return RunImport<TopcatMapping>(model);
////            if (model.Id == 1) return RunImport<ActivitiesMapping>(model);
////            if (model.Id == 2) return RunImport<MeshMapping>(model);
////            if (model.Id == 3) return RunImport<PubCatMapping>(model);
////            if (model.Id == 4) return RunImport<MeowMapping>(model);
////            if (model.Id == 5) return RunImport<SeabedSurveyMapping>(model);
//            if (arg == "marinerecorder") return MarineRecorderMapping);
//            //throw new ArgumentException("Invalid import id");
//        }
    }

    
}
