using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Catalogue.Data.Test;
using Catalogue.Data.Write;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Clone;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;
using Catalogue.Data;
using static Catalogue.Tests.TestUserInfo;

namespace Catalogue.Tests.Slow.Catalogue.Data.Indexes
{
    public class records_with_duplicate_title_checker_index_tests : CleanDbTest
    {
        [Test]
        public void should_count_records_with_duplicate_titles()
        {
            using (var db = ReusableDocumentStore.OpenSession())
            {
                var service = new RecordService(db, new RecordValidator());

                var record1 = new Record().With(r =>
                {
                    r.Id = Helpers.AddCollection("7ce85158-f6f9-491d-902e-b3f2c8bb5264");
                    r.Path = @"X:\path\to\duplicate\record\1";
                    r.Gemini = new Metadata().With(m =>
                    {
                        m.Title = "This is a duplicate record";
                        m.Keywords.Add(new MetadataKeyword
                            {Vocab = "http://vocab.jncc.gov.uk/jncc-domain", Value = "Marine"});
                        m.Keywords.Add(new MetadataKeyword
                            {Vocab = "http://vocab.jncc.gov.uk/jncc-category", Value = "Example"});
                    });
                });
                var record2 = new Record().With(r =>
                {
                    r.Id = Helpers.AddCollection("afb4ebbf-4286-47ed-b09f-a4d40af139e1");
                    r.Path = @"X:\path\to\duplicate\record\2";
                    r.Gemini = new Metadata().With(m =>
                    {
                        m.Title = "This is a duplicate record";
                        m.Keywords.Add(new MetadataKeyword
                            {Vocab = "http://vocab.jncc.gov.uk/jncc-domain", Value = "Marine"});
                        m.Keywords.Add(new MetadataKeyword
                            {Vocab = "http://vocab.jncc.gov.uk/jncc-category", Value = "Example"});
                    });
                });

                service.Insert(record1, TestUser);
                service.Insert(record2, TestUser);

                db.SaveChanges();
                WaitForIndexing(ReusableDocumentStore);

                var results = db
                    .Query<RecordsWithDuplicateTitleCheckerIndex.Result, RecordsWithDuplicateTitleCheckerIndex>()
                    .Where(x => x.Count > 1)
                    .Take(100)
                    .ToList();

                // looks like we have some duplicates created by the seeder!
                results.Count.Should()
                    .BeInRange(1, 10); // perhaps prevent more duplicate titles being seeded in the future!
                results.Should().Contain(r => r.Title == "This is a duplicate record");
            }
        }
    }
}
