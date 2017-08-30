using System.IO;
using System.Reflection;
using Catalogue.Data.Model;
using Catalogue.Data.Test;
using NUnit.Framework;
using Raven.Imports.Newtonsoft.Json;

namespace Catalogue.Tests.Explicit.Catalogue.Robot
{
    public class robot_uploader_specs
    {
        [Test][Ignore]
        public void metadata_document_generated_correctly()
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                @"TestResources\records\721643b8-7e42-40ca-87d9-23f19221238e.json");
            var record =
                JsonConvert.DeserializeObject<Record>(
                    File.ReadAllText(path));

            var store = new InMemoryDatabaseHelper().Create();
            using (var db = store.OpenSession())
            {
                db.Store(record);
            }
        }
    }
}
