using System;
using System.Linq;
using Catalogue.Data.Model;
using Catalogue.Web.Controllers;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Slow.Catalogue.Web.Controllers.Search
{
    class search_helper_specs : DatabaseTestFixture
    {
        [Test]
        public void keyword_search_test()
        {
            var helper = new RecordQueryer(Db);

            var input = new RecordQueryInputModel
                {
                    Q = "",
                    K = new [] { "vocab.jncc.gov.uk/jncc-category/Seabed Habitat Maps" },
                    P = 0,
                    N = 25,
                    D = null
                };

            helper.SearchQuery(input).Results.Count().Should().Be(25);
        }

        [Test]
        public void search_on_last_modifed_date()
        {
            //get one record
            var helper = new RecordQueryer(Db);
            var input = new RecordQueryInputModel
            {
                Q = "sh",
                K = null,
                P = 0,
                N = 25,
                D = null
            };

            var record = helper.RecordQuery(input).First();
            record.Notes = record.Notes + "SOME TEXT";

            var changedTime = DateTime.Now;
            
            Db.Store(record);
            Db.SaveChanges();

            input.D = changedTime;

            var id = record.Id;

            var record2 = Db.Load<Record>(id);

            var metadata = Db.Advanced.GetMetadataFor(record2);

            metadata.Value<DateTime>("Last-Modified").Should().BeAfter(changedTime);

            //helper.RecordQuery(input).Count().Should().Be(1);

        }
    }
}
