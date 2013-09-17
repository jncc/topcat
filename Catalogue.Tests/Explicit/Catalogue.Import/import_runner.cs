using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Import;
using Catalogue.Import.Formats;
using Catalogue.Import.Utilities;
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
            store.Initialize();

            var importer = new Importer<DefaultFormat>(new FileSystem(), store);
            importer.Import(@"c:\work\mesh\mesh.csv");
        }
    }
}
