using System.Collections.Generic;
using System.Linq;
using Catalogue.Data.Model;
using NUnit.Framework;

namespace Catalogue.Tests.Explicit
{
    internal class ad_hoc_queries : DatabaseTestFixture
    {
        [Explicit, Test]
        public void weird_characters()
        {
            List<Record> records = Db.Advanced.LuceneQuery<Record>("Records/Search")
                .Search("Abstract", "Loch Maddy monitoring trials")
                .ToList();
        }
    }
}