using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Catalogue.Data.Query;
using Catalogue.Data.Test;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Clone;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client;
using Raven.Abstractions.Extensions;

namespace Catalogue.Tests.Explicit
{
    class reproduce_unexpected_query_result
    {
        [Test]
        public void ShouldGetRightResults()
        {
            var documentStore = DatabaseFactory.InMemory(8889);

            new RecordIndex().Execute(documentStore);

            using (var db = documentStore.OpenSession())
            {
                new[] { "foo", "foo (bar)" }.Select(MakeRecord).ForEach(db.Store);

                db.SaveChanges();
            }

            RavenUtility.WaitForIndexing(documentStore);

            using (var db = documentStore.OpenSession())
            {
                Action<string, int> testCase = (k, n) =>
                {
//                    var query = db.Query<MyIndex.Result, MyIndex>()
//                        .Where(r => r.Keywords.Contains(k));
//                    var results = query.As<Record>().Take(10).ToList();
//                    results.Count.Should().Be(n);

                    var input = EmptyQuery().With(q => q.K = new[] { k });
                    var output = new RecordQueryer(db).SearchQuery(input);
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
                }
            };
        }

        RecordQueryInputModel EmptyQuery()
        {
            return new RecordQueryInputModel
            {
                Q = "",
                K = new string[0],
                P = 0,
                N = 25,
            };

        }
    }

}
