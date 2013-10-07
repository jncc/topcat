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
using Raven.Client;
using Raven.Client.Embedded;
using Raven.Client.Indexes;

namespace Catalogue.Tests.Explicit
{
    class exploratory_search_tests : DatabaseTestFixture
    {
        [Explicit, Test]
        public void can_search()
        {
            // sanity check the seed data has loaded
            Db.Query<Record>().Count().Should().BeGreaterThan(100);
            

            var list = Db.Advanced.LuceneQuery<Record>("Records/Search")
                .Search("Title", "North")
                .ToList();

            foreach (var record in list)
            {
                Console.WriteLine(record.Gemini.Title);
            }

            list.Count.Should().BeGreaterThan(0);
        }
    }
}
