using System.IO;
using System.Reflection;
using System.Resources;
using Catalogue.Data.Model;
using Catalogue.Data.Test;
using Catalogue.Robot.Publishing.OpenData;
using FluentAssertions;
using NUnit.Framework;
using Raven.Imports.Newtonsoft.Json;

namespace Catalogue.Tests.Explicit.Catalogue.Robot
{
    public class opendataxmlhelper_specs
    {
        [Test]
        public void metadata_document_generated_correctly()
        {
            var inputRecord = GetInputFileContents(@"records.721643b8-7e42-40ca-87d9-23f19221238e.json");
            var record = JsonConvert.DeserializeObject<Record>(inputRecord);
            var expectedWaf = GetInputFileContents(@"wafs.721643b8-7e42-40ca-87d9-23f19221238e.xml");

            var xmlHelper = new OpenDataXmlHelper();
            var bytes = xmlHelper.GetMetadataDocument(record, "http://data.jncc.gov.uk/data/721643b8-7e42-40ca-87d9-23f19221238e-Scotia-Herring-Acoustic-Grab.zip");
            var actualWaf = System.Text.Encoding.Default.GetString(bytes);

            actualWaf.Should().Be(expectedWaf);
        }

        private string GetInputFileContents(string filename)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string path = "Catalogue.Tests.TestResources." + filename;
            var resourceStream = assembly.GetManifestResourceStream(path);
            var sr = new StreamReader(resourceStream);
            var contents = sr.ReadToEnd();
            sr.Close();
            resourceStream.Close();
            return contents;
        }
    }
}
