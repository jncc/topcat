using Catalogue.Data.Model;
using Catalogue.Data.Query;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Clone;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Catalogue.Tests.Explicit
{
    class reproduce_unexpected_query_result : SeededDbTest
    {
        [Test, Explicit]
        public void ShouldGetRightResults()
        {
            using (var db = ReusableDocumentStore.OpenSession())
            {
                var records = new[] {"foo", "foo (bar)"}.Select(MakeRecord);

                foreach (var record in records)
                {
                    db.Store(record);
                }

                db.SaveChanges();
            }

            WaitForIndexing(ReusableDocumentStore);

            using (var db = ReusableDocumentStore.OpenSession())
            {
                Action<string, int> testCase = (k, n) =>
                {
//                    var query = db.Query<MyIndex.Result, MyIndex>()
//                        .Where(r => r.Keywords.Contains(k));
//                    var results = query.As<Record>().Take(10).ToList();
//                    results.Count.Should().Be(n);

                    var input = EmptyQuery().With(q => q.F.Keywords = new[] { k });
                    var output = new RecordQueryer(db).Search(input);
                    output.Total.Should().Be(n);
                };

                // todo how to deal with empty keyword?
//                testCase("", 0);
                testCase("http://vocab/xxx", 0);
//                testCase("http://vocab/foo", 1);
//                testCase("http://vocab/foo (xxx)", 0);
//                testCase("http://vocab/foo (bar)", 1);
//                testCase("http://vocab/foo - (xxx)", 0);
            }
        }

        Record MakeRecord(string keyword)
        {
            return new Record
            {
                Gemini = new Metadata
                {
                    Keywords = new List<MetadataKeyword>
                    {
	                    new MetadataKeyword { Vocab = "http://vocab", Value = keyword }
	                }
                },
                Footer = new Footer()
            };
        }

        RecordQueryInputModel EmptyQuery()
        {
            return new RecordQueryInputModel
            {
                Q = "",
                F = new FilterOptions{ Keywords = new string[0] },
                P = 0,
                N = 25,
            };

        }
    }

}
