using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Indexes;
using Catalogue.Gemini.Model;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Slow.Catalogue.Data.Indexes
{
    class jncc_categories_index_spec : DatabaseTestFixture
    {
        [Test]
        public void should_be_able_to_get_jncc_category_record_counts()
        {
            var results = Db.Query<JnccCategoriesIndex.Result, JnccCategoriesIndex>().ToList();

            results.Count.Should().BeGreaterOrEqualTo(2);
            results.Where(r => r.CategoryName == "Seabed Habitat Maps")
                .Select(r => r.RecordCount)
                .Single()
                .Should().BeGreaterThan(100);
        }
    }
}
