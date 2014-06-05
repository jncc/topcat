using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using Catalogue.Data.Import;
using Catalogue.Data.Import.Mappings;
using Catalogue.Data.Seed;
using NUnit.Framework.Constraints;
using Raven.Client;

namespace Catalogue.Web.Admin
{
    public class AdminController : ApiController
    {

        public class FileSpec
        {
            public string Path { get; set; }         
        }

        public Boolean PostImport(FileSpec file)
        {

            using (var db = WebApiApplication.DocumentStore.OpenSession())
            {
                var importer = Importer.CreateImporter<ActivitiesMapping>(db);
                //importer.SkipBadRecords = true; // todo remove this
                importer.Import(file.Path);
                db.SaveChanges();
            }
            return true;
        }

        
       /* [HttpPost]
        [ActionName("seedMesh")]
        public void PostSeedMesh()
        {
            Seeder.Seed(WebApiApplication.DocumentStore);
        }
        * */

        [ActionName("bool")]
        public void GetBool()
        {
            IDocumentStore documentStore = WebApiApplication.DocumentStore;
            
            throw new IOException("My Test Message");
        
        }
    }
}
