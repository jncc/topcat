using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client;

namespace Catalogue.Tests.Explicit.Catalogue.Data.Indexes
{
//    class my_record_index_tests : DatabaseTestFixture
//    {
//        [Test]
//        public void blah()
//        {
//            var query = Db.SearchQuery<MyRecordIndex.Result, MyRecordIndex>()
//                .Search(r => r.Title, "broad")
//                .Where(r => r.Keywords.Contains("http://vocab.jncc.gov.uk/reference-manager-code/R2290"))
//                .As<Record>();
//
//            var results = query.ToList();
//            results.Should().HaveCount(2);
//            Console.WriteLine(results.Count);
//        }
//
//    }
//
//    public class MyRecordIndex : AbstractIndexCreationTask<Record, MyRecordIndex.Result>
//    {
//        public class Result
//        {
//            public string Title { get; set; }
//            public string[] Keywords { get; set; }
//        }
//
//        public MyRecordIndex()
//        {
//            Map = records => from record in records
//                             select new
//                             {
//                                 Title = record.Gemini.Title,
//                                 Keywords = record.Gemini.Keywords.Select(k => k.Vocab + "/" + k.Value)
//                             };
//
//            Analyze(x => x.Title, typeof(StandardAnalyzer).AssemblyQualifiedName);
//            Stores.Add(x => x.Title, FieldStorage.Yes);
//            TermVector(x => x.Title, FieldTermVector.WithPositionsAndOffsets);
//        }
//    }

}
