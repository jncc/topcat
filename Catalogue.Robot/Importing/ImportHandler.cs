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

        public void Import(string[] args)
        {
            string mapping = args[1]; // -mapping "marinerecorder"
            string file = args[2];    // -file "C:\work\file.csv"
            var importer = global::Catalogue.Data.Import.Importer.CreateImporter<MarineRecorderMapping>(db);
            importer.SkipBadRecords = false; // --skip-bad
            importer.Import(file.Trim('"'));
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
