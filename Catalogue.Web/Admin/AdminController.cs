using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Catalogue.Data.Import;
using Catalogue.Data.Import.Mappings;
using Catalogue.Data.Seed;

namespace Catalogue.Web.Admin
{
    public class AdminController : ApiController
    {
        public Boolean PostImport([FromBody]string file)
        {
            using (var db = WebApiApplication.DocumentStore.OpenSession())
            {
                var importer = Importer.CreateImporter<ActivitiesMapping>(db);
                //importer.SkipBadRecords = true; // todo remove this
                importer.Import(file);
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
        public Boolean GetBool()
        {
            return true;
        }
    }
}
