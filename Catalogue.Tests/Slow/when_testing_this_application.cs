using System;
using System.IO;
using System.Linq;
using Catalogue.Data.Model;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client;

namespace Catalogue.Tests.Slow
{
    public class when_testing_this_application : DatabaseTestFixture
    {
        [Test]
        public void should_be_able_to_query_test_data()
        {
            using (IDocumentSession db = ReusableDocumentStore.OpenSession())
            {
                db.Query<Record>().Count().Should().BePositive();
            }
        }


        [Explicit, Test]
        public void make_an_xml()
        {
            var record = Db.Load<Record>(new Guid("679434f5-baab-47b9-98e4-81c8e3a1a6f9"));
            record.Gemini.ResourceLocator = String.Format("http://example.com/{0}", record.Id);
            var xml = new global::Catalogue.Gemini.Encoding.XmlEncoder().Create(record.Id, record.Gemini);
            string filename = "topcat-record-" + record.Id.ToString().ToLower() + ".xml";
            xml.Save(Path.Combine(@"C:\work", filename));

        }

    }
}