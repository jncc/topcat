using System;
using Catalogue.Data.Model;
using Microsoft.XmlDiffPatch;
using Newtonsoft.Json;
using NUnit.Framework;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using Catalogue.Robot.Publishing.Gov;

namespace Catalogue.Tests.Slow.Catalogue.Robot
{
    public class publishingxmlhelper_specs
    {
        [Test]
        public void metadata_document_generated_correctly_for_record_with_file_resource()
        {
            var record = GetRecordFromFile("721643b8-7e42-40ca-87d9-23f19221238e", @"records.721643b8-7e42-40ca-87d9-23f19221238e.json");
            var expectedXmlDoc = GetInputFileAsXmlDoc(@"wafs.721643b8-7e42-40ca-87d9-23f19221238e.xml");

            var xmlHelper = new XmlHelper();
            var actualWaf = xmlHelper.GetMetadataDocument(record);
            var actualXmlDoc = GetByteArrayAsXmlDoc(actualWaf);

            XmlDiff xmlDiff = new XmlDiff();
            Assert.True(xmlDiff.Compare(expectedXmlDoc, actualXmlDoc));
        }

        [Test]
        public void metadata_document_generated_correctly_for_record_with_multiple_resources()
        {
            var record = GetRecordFromFile("00b0b44c-a062-4a25-b344-2be12b03a6b5", @"records.00b0b44c-a062-4a25-b344-2be12b03a6b5.json");
            var expectedXmlDoc = GetInputFileAsXmlDoc(@"wafs.00b0b44c-a062-4a25-b344-2be12b03a6b5.xml");

            var xmlHelper = new XmlHelper();
            var actualWaf = xmlHelper.GetMetadataDocument(record);
            var actualXmlDoc = GetByteArrayAsXmlDoc(actualWaf);

            XmlDiff xmlDiff = new XmlDiff();
            Assert.True(xmlDiff.Compare(expectedXmlDoc, actualXmlDoc));
        }

        [Test]
        public void metadata_document_generated_correctly_for_record_with_url_resource()
        {
            var record = GetRecordFromFile("9d9775da-44b1-4b96-9302-c842958e9130", @"records.9d9775da-44b1-4b96-9302-c842958e9130.json");
            var expectedXmlDoc = GetInputFileAsXmlDoc(@"wafs.9d9775da-44b1-4b96-9302-c842958e9130.xml");

            var xmlHelper = new XmlHelper();
            var actualWaf = xmlHelper.GetMetadataDocument(record);
            var actualXmlDoc = GetByteArrayAsXmlDoc(actualWaf);

            XmlDiff xmlDiff = new XmlDiff();
            Assert.True(xmlDiff.Compare(expectedXmlDoc, actualXmlDoc));
        }

        [Test]
        public void metadata_document_generated_correctly_for_record_without_resources()
        {
            var record = GetRecordFromFile("c6f3632d-8789-460b-a09d-c132841a7190", @"records.c6f3632d-8789-460b-a09d-c132841a7190.json");
            var expectedXmlDoc = GetInputFileAsXmlDoc(@"wafs.c6f3632d-8789-460b-a09d-c132841a7190.xml");

            var xmlHelper = new XmlHelper();
            var actualWaf = xmlHelper.GetMetadataDocument(record);
            var actualXmlDoc = GetByteArrayAsXmlDoc(actualWaf);

            XmlDiff xmlDiff = new XmlDiff();
            Assert.True(xmlDiff.Compare(expectedXmlDoc, actualXmlDoc));
        }

        [Test]
        public void metadata_document_generated_correctly_for_record_with_additional_information()
        {
            var record = GetRecordFromFile("4cb2cca3-ec95-4962-9618-8556d88390fd", @"records.4cb2cca3-ec95-4962-9618-8556d88390fd.json");
            var expectedXmlDoc = GetInputFileAsXmlDoc(@"wafs.4cb2cca3-ec95-4962-9618-8556d88390fd.xml");

            var xmlHelper = new XmlHelper();
            var actualWaf = xmlHelper.GetMetadataDocument(record);
            var actualXmlDoc = GetByteArrayAsXmlDoc(actualWaf);

            XmlDiff xmlDiff = new XmlDiff();
            Assert.True(xmlDiff.Compare(expectedXmlDoc, actualXmlDoc));
        }

        [Test]
        public void record_publishable_to_hub_and_gov()
        {
            var record = GetRecordFromFile("64b5f778-c098-4474-a36e-7f4b2bdfd10b", @"records.64b5f778-c098-4474-a36e-7f4b2bdfd10b.json");
            var expectedXmlDoc = GetInputFileAsXmlDoc(@"wafs.64b5f778-c098-4474-a36e-7f4b2bdfd10b.xml");

            var xmlHelper = new XmlHelper();
            var actualWaf = xmlHelper.GetMetadataDocument(record);
            var actualXmlDoc = GetByteArrayAsXmlDoc(actualWaf);

            XmlDiff xmlDiff = new XmlDiff();
            Assert.True(xmlDiff.Compare(expectedXmlDoc, actualXmlDoc));
        }

        [Test]
        public void record_unpublishable_to_hub_and_publishable_to_gov()
        {
            var record = GetRecordFromFile("85a9bbdc-2397-4f7c-a71e-0480b26b8807", @"records.85a9bbdc-2397-4f7c-a71e-0480b26b8807.json");
            var expectedXmlDoc = GetInputFileAsXmlDoc(@"wafs.85a9bbdc-2397-4f7c-a71e-0480b26b8807.xml");

            var xmlHelper = new XmlHelper();
            var actualWaf = xmlHelper.GetMetadataDocument(record);
            var actualXmlDoc = GetByteArrayAsXmlDoc(actualWaf);

            XmlDiff xmlDiff = new XmlDiff();
            Assert.True(xmlDiff.Compare(expectedXmlDoc, actualXmlDoc));
        }

        [Test]
        public void waf_index_document_generated_correctly()
        {
            var record = GetRecordFromFile("721643b8-7e42-40ca-87d9-23f19221238e", @"records.721643b8-7e42-40ca-87d9-23f19221238e.json");
            var initialIndex = GetInputFileContents(@"wafs.index_initial.html");
            var expectedIndexDoc = GetInputFileAsXmlDoc(@"wafs.index_expected.html");

            var xmlHelper = new XmlHelper();
            var actualWaf = xmlHelper.UpdateWafIndexDocument(record, initialIndex);
            var actualIndexDoc = GetStringAsXmlDoc(actualWaf);

            XmlDiff xmlDiff = new XmlDiff();
            Assert.True(xmlDiff.Compare(expectedIndexDoc, actualIndexDoc));
        }

        private string GetInputFileContents(string filename)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string path = "Catalogue.Tests.TestResources." + filename;

            using (var rs = assembly.GetManifestResourceStream(path))
            using (var sr = new StreamReader(rs))
            {
                var contents = sr.ReadToEnd();
                return contents;
            }
        }

        private Record GetRecordFromFile(string id, string filename)
        {
            var fileContents = GetInputFileContents(filename);
            var record = JsonConvert.DeserializeObject<Record>(fileContents);
            record.Id = id;

            return record;
        }

        private XmlDocument GetInputFileAsXmlDoc(string filename)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string path = "Catalogue.Tests.TestResources." + filename;

            using (var rs = assembly.GetManifestResourceStream(path))
            using (var sr = new StreamReader(rs))
            {
                var actualIndexDoc = new XmlDocument();
                actualIndexDoc.Load(sr);

                return actualIndexDoc;
            }
        }

        private XmlDocument GetStringAsXmlDoc(string contents)
        {
            return GetByteArrayAsXmlDoc(Encoding.UTF8.GetBytes(contents));
        }

        private XmlDocument GetByteArrayAsXmlDoc(byte[] contents)
        {
            using (var ms = new MemoryStream(contents))
            using (var sr = new StreamReader(ms))
            {
                var actualIndexDoc = new XmlDocument();
                actualIndexDoc.Load(sr);
                return actualIndexDoc;
            }
        }
    }
}
