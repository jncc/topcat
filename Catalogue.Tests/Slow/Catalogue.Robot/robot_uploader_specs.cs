using System.IO;
using System.Reflection;
using System.Resources;
using Catalogue.Data.Model;
using Catalogue.Data.Test;
using NUnit.Framework;
using Raven.Imports.Newtonsoft.Json;

namespace Catalogue.Tests.Explicit.Catalogue.Robot
{
    public class robot_uploader_specs
    {
        [Test]
        public void metadata_document_generated_correctly()
        {
            var fileContents = GetInputFileContents(@"TestResources\records\721643b8-7e42-40ca-87d9-23f19221238e.json");
            var record =
                JsonConvert.DeserializeObject<Record>(
                    File.ReadAllText(fileContents));

            var store = new InMemoryDatabaseHelper().Create();
            using (var db = store.OpenSession())
            {
                db.Store(record);
            }
        }

        private string GetInputFileContents(string filename)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string path = "Catalogue.Robot";
            var sr = new StreamReader(assembly.GetManifestResourceStream(path + "." + filename));
            var contents = sr.ReadToEnd();
            sr.Close();
            return contents;
        }
    }
}
