using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Import;
using Catalogue.Data.Import.Mappings;
using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Tests.Utility;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client.Embedded;
using Raven.Client.Indexes;

namespace Catalogue.Tests.Explicit
{
    class exploratory_search_tests
    {
        [Explicit, Test]
        public void can_search()
        {
            var store = new EmbeddableDocumentStore { RunInMemory = true };
            store.Initialize();
            
            using (var db = store.OpenSession())
            {
                string resource = "Catalogue.Data.Seed.mesh.csv";
                var s = Assembly.GetAssembly(typeof(Record)).GetManifestResourceStream(resource);

                using (var reader = new StreamReader(s))
                {
                    var importer = new Importer<MeshMapping>(new FileSystem(), new RecordService(db));
                    importer.Import(reader);
                }

                db.SaveChanges();
            }

            IndexCreation.CreateIndexes(typeof(Record).Assembly, store);
            RavenUtility.WaitForIndexing(store);

            using (var db = store.OpenSession())
            {
                db.Query<Record>().Count().Should().BeGreaterThan(100);
            }
        }
    }
}
