using Catalogue.Data.Model;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client.Documents.Session;
using System.Linq;
using Raven.Client.Documents;
using Raven.TestDriver;

namespace Catalogue.Tests
{
    public class raven4test : RavenTestDriver
    {

        [Test]
        public void should_be_able_to_query_test_data()
        {
            ConfigureServer(new TestServerOptions
            {
                FrameworkVersion = "2.1.5",
                ServerUrl = "http://localhost:8090"
            });
            using (var store = GetDocumentStore())
            {
                using (var s = store.OpenSession())
                {
                    s.Store(new {name = "hello"});
                    s.SaveChanges();
                }

                store.Dispose();
            }
        }

    }
}