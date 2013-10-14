using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using Catalogue.Tests.Utility;
using NUnit.Framework;

namespace Catalogue.Tests.Explicit
{
    class ad_hoc_queries : DatabaseTestFixture
    {
        [Explicit, Test]
        public void weird_characters()
        {
            var records = Db.Advanced.LuceneQuery<Record>("Records/Search")
                            .Search("Abstract", "Loch Maddy monitoring trials")
                            .ToList();


        }
    }
}
