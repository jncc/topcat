using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Import;
using Catalogue.Data.Import.Mappings;
using Catalogue.Data.Write;
using Catalogue.Tests.Utility;
using NUnit.Framework;
using Raven.Client.Document;

namespace Catalogue.Tests.Explicit.Catalogue.Import
{
    class import_runner
    {
        [Explicit]
        [Test]
        public void run()
        {
            var store = new DocumentStore();
            store.ParseConnectionString("Url=http://localhost:8888/");
            store.Initialize();

            using (var db = store.OpenSession())
            {
                var importer = new Importer<MeshMapping>(new FileSystem(), new RecordService(db));
                importer.Import(@"C:\Users\Pete\Desktop\mesh.csv");
            }
        }
    }
}
