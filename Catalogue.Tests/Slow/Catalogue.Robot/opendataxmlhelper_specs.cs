using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using Catalogue.Data;
using Catalogue.Data.Model;
using Catalogue.Robot.Publishing.OpenData;
using Microsoft.XmlDiffPatch;
using NUnit.Framework;
using Newtonsoft.Json;

namespace Catalogue.Tests.Slow.Catalogue.Robot
{
    public class opendataxmlhelper_specs
    {
        [Test]
        public void metadata_document_generated_correctly_for_record_with_resources()
        {
            var record = GetRecordFromFile(Helpers.AddCollection("721643b8-7e42-40ca-87d9-23f19221238e"), @"records.721643b8-7e42-40ca-87d9-23f19221238e.json");
            var expectedXmlDoc = GetInputFileAsXmlDoc(@"wafs.721643b8-7e42-40ca-87d9-23f19221238e.xml");

            var xmlHelper = new OpenDataXmlHelper();
            var actualWaf = xmlHelper.GetMetadataDocument(record, "http://data.jncc.gov.uk/data/721643b8-7e42-40ca-87d9-23f19221238e-Scotia-Herring-Acoustic-Grab");
            var actualXmlDoc = GetByteArrayAsXmlDoc(actualWaf);

            XmlDiff xmlDiff = new XmlDiff();
            Assert.True(xmlDiff.Compare(expectedXmlDoc, actualXmlDoc));
        }

        [Test]
        public void metadata_document_generated_correctly_for_record_without_resources()
        {
            var record = GetRecordFromFile(Helpers.AddCollection("c6f3632d-8789-460b-a09d-c132841a7190"), @"records.c6f3632d-8789-460b-a09d-c132841a7190.json");
            var expectedXmlDoc = GetInputFileAsXmlDoc(@"wafs.c6f3632d-8789-460b-a09d-c132841a7190.xml");

            var xmlHelper = new OpenDataXmlHelper();
            var actualWaf = xmlHelper.GetMetadataDocument(record, "");
            var actualXmlDoc = GetByteArrayAsXmlDoc(actualWaf);

            XmlDiff xmlDiff = new XmlDiff();
            Assert.True(xmlDiff.Compare(expectedXmlDoc, actualXmlDoc));
        }

        [Test]
        public void metadata_document_generated_correctly_for_record_with_additional_information()
        {
            var record = GetRecordFromFile(Helpers.AddCollection("4cb2cca3-ec95-4962-9618-8556d88390fd"), @"records.4cb2cca3-ec95-4962-9618-8556d88390fd.json");
            var expectedXmlDoc = GetInputFileAsXmlDoc(@"wafs.4cb2cca3-ec95-4962-9618-8556d88390fd.xml");

            var xmlHelper = new OpenDataXmlHelper();
            var actualWaf = xmlHelper.GetMetadataDocument(record, "");
            var actualXmlDoc = GetByteArrayAsXmlDoc(actualWaf);

            XmlDiff xmlDiff = new XmlDiff();
            Assert.True(xmlDiff.Compare(expectedXmlDoc, actualXmlDoc));
        }

        [Test]
        public void waf_index_document_generated_correctly()
        {
            var record = GetRecordFromFile(Helpers.AddCollection("721643b8-7e42-40ca-87d9-23f19221238e"), @"records.721643b8-7e42-40ca-87d9-23f19221238e.json");
            var initialIndex = GetInputFileContents(@"wafs.index_initial.html");
            var expectedIndexDoc = GetInputFileAsXmlDoc(@"wafs.index_expected.html");

            var xmlHelper = new OpenDataXmlHelper();
            var actualWaf = xmlHelper.UpdateWafIndexDocument(record, initialIndex);
            var actualIndexDoc = GetStringAsXmlDoc(actualWaf);

            XmlDiff xmlDiff = new XmlDiff();
            Assert.True(xmlDiff.Compare(expectedIndexDoc, actualIndexDoc));
        }

        [Test]
        public void metadata_pointofcontact_should_be_redacted()
        {
            var record = GetRecordFromFile(Helpers.AddCollection("c6f3632d-8789-460b-a09d-c132841a7190"), @"records.c6f3632d-8789-460b-a09d-c132841a7190.json");
            var expectedXmlDoc = GetInputFileAsXmlDoc(@"wafs.c6f3632d-8789-460b-a09d-c132841a7190.xml");

            record.Gemini.MetadataPointOfContact.Name = "Bob Flemming"; // this name should not be in the output xml

            var xmlHelper = new OpenDataXmlHelper();
            var actualWaf = xmlHelper.GetMetadataDocument(record, "");
            var actualXmlDoc = GetByteArrayAsXmlDoc(actualWaf);

            XmlDiff xmlDiff = new XmlDiff();
            Assert.True(xmlDiff.Compare(expectedXmlDoc, actualXmlDoc));
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
