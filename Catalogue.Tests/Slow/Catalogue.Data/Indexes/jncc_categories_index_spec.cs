using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Indexes;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Slow.Catalogue.Data.Indexes
{
    class jncc_categories_index_spec : DatabaseTestFixture
    {
        [Test]
        public void blah()
        {
            var results = Db.Query<JnccCategoriesIndex.Result, JnccCategoriesIndex>().ToList();
            results.Should().NotBeEmpty();
        }
    }
}
