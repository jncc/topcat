using Catalogue.Data;
using Catalogue.Data.Model;
using Catalogue.Gemini.Helpers;
using Catalogue.Gemini.Templates;
using Catalogue.Robot.Publishing.Hub;
using Catalogue.Utilities.Clone;
using Catalogue.Utilities.Collections;
using FluentAssertions;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Catalogue.Gemini.Model;
using Catalogue.Robot.Publishing;

namespace Catalogue.Tests.Slow.Catalogue.Robot
{
    public class hub_message_helper_specs
    {
        private Env env;

        [OneTimeSetUp]
        public void Init()
        {
            env = new Env(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "TestResources\\.env.test");
        }

        private Record readyToUploadRecord = new Record().With(r =>
        {
            r.Id = Helpers.AddCollection("0545c14b-e7fd-472d-8575-5bb75034945f");
            r.Path = @"X:\path\to\uploader\test";
            r.Validation = Validation.Gemini;
            r.Gemini = Library.Example().With(m =>
            {
                m.Title = "Test record";
                m.Abstract = "This is a test record";
                m.Keywords = new StringPairList
                    {
                        {"", "Vocabless record"},
                        {"http://vocab.jncc.gov.uk/jncc-domain", "Terrestrial"},
                        {"http://vocab.jncc.gov.uk/jncc-category", "Example Collection"}
                    }
                    .ToKeywordList();
                m.DatasetReferenceDate = "2015-04-14";
                m.MetadataDate = new DateTime(2017, 09, 26);
            });
            r.Publication = new PublicationInfo
            {
                Assessment = new AssessmentInfo
                {
                    Completed = true,
                    CompletedOnUtc = new DateTime(2017, 09, 25)
                },
                SignOff = new SignOffInfo
                {
                    DateUtc = new DateTime(2017, 09, 26)
                },
                Data = null,
                Target = new TargetInfo
                {
                    Hub = new HubPublicationInfo
                    {
                        Url = "http://hub.jncc.gov.uk/assets/0545c14b-e7fd-472d-8575-5bb75034945f",
                        Publishable = true
                    },
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true
                    }
                }
            };
            r.Footer = new Footer();
        });

        [Test]
        public void queue_message_with_no_resources()
        {
            var record = readyToUploadRecord.With(r => r.Publication.Data = null);
            var fileHelperMock = new Mock<IFileHelper>();
            var hubMessageHelper = new HubMessageConverter(env, fileHelperMock.Object);

            var queueMessage = hubMessageHelper.ConvertRecordToQueueMessage(record);
            var actualObject = JObject.Parse(queueMessage);

            var expectedObject = JObject.Parse(
                @"{
                    ""verb"":""upsert"",
                    ""index"":""topcatdev"",
                    ""document"":{
                        ""id"":""0545c14b-e7fd-472d-8575-5bb75034945f"",
                        ""site"":""datahub"",
                        ""title"":""Test record"",
                        ""content"":""This is a test record"",
                        ""url"":""http://hub.jncc.gov.uk/assets/0545c14b-e7fd-472d-8575-5bb75034945f"",
                        ""resource_type"":""dataset"",
                        ""keywords"":[
                            {""value"":""Vocabless record"",""vocab"":""""},
                            {""value"":""Terrestrial"",""vocab"":""http://vocab.jncc.gov.uk/jncc-domain""},
                            {""value"":""Example Collection"",""vocab"":""http://vocab.jncc.gov.uk/jncc-category""}
                        ],
                        ""published_date"":""2015-04-14""
                    },
                    ""resources"":[]
                }");

            JToken.DeepEquals(expectedObject, actualObject).Should().BeTrue();
        }

        [Test]
        public void queue_message_with_a_non_pdf_file_resource()
        {
            var record = readyToUploadRecord.With(r => r.Publication.Data = new DataInfo
            {
                Resources = new List<Resource> {
                    new Resource {
                        Name = "A csv resource",
                        Path = "C:\\work\\test.csv",
                        PublishedUrl = "http://data.jncc.gov.uk/data/0545c14b-e7fd-472d-8575-5bb75034945f/test.csv"
                    }
                }
            });

            var fileHelperMock = new Mock<IFileHelper>();
            var hubMessageHelper = new HubMessageConverter(env, fileHelperMock.Object);
            fileHelperMock.Setup(x => x.GetFileExtensionWithoutDot(It.IsAny<string>())).Returns("csv");
            fileHelperMock.Setup(x => x.GetFileSizeInBytes(It.IsAny<string>())).Returns(5);

            var queueMessage = hubMessageHelper.ConvertRecordToQueueMessage(record);
            var actualObject = JObject.Parse(queueMessage);

            var expectedObject = JObject.Parse(
                @"{
                    ""verb"":""upsert"",
                    ""index"":""topcatdev"",
                    ""document"":{
                        ""id"":""0545c14b-e7fd-472d-8575-5bb75034945f"",
                        ""site"":""datahub"",
                        ""title"":""Test record"",
                        ""content"":""This is a test record"",
                        ""url"":""http://hub.jncc.gov.uk/assets/0545c14b-e7fd-472d-8575-5bb75034945f"",
                        ""resource_type"":""dataset"",
                        ""keywords"":[
                            {""value"":""Vocabless record"",""vocab"":""""},
                            {""value"":""Terrestrial"",""vocab"":""http://vocab.jncc.gov.uk/jncc-domain""},
                            {""value"":""Example Collection"",""vocab"":""http://vocab.jncc.gov.uk/jncc-category""}
                        ],
                        ""published_date"":""2015-04-14""
                    },
                    ""resources"":[
                        {
                            ""title"":""A csv resource"",
                            ""content"":""This is a test record"",
                            ""url"":""http://data.jncc.gov.uk/data/0545c14b-e7fd-472d-8575-5bb75034945f/test.csv"",
                            ""file_extension"":""csv"",
                            ""file_bytes"":5
                        }
                    ]
                }");

            JToken.DeepEquals(expectedObject, actualObject).Should().BeTrue();
        }

        [Test]
        public void queue_message_with_publication_resource_type()
        {
            var record = readyToUploadRecord.With(r => r.Publication.Data = new DataInfo
            {
                Resources = new List<Resource> {
                    new Resource {
                        Name = "A csv resource",
                        Path = "C:\\work\\test.csv",
                        PublishedUrl = "http://data.jncc.gov.uk/data/0545c14b-e7fd-472d-8575-5bb75034945f/test.csv"
                    }
                }
            })
            .With(r => r.Gemini.ResourceType = "publication");

            var fileHelperMock = new Mock<IFileHelper>();
            var hubMessageHelper = new HubMessageConverter(env, fileHelperMock.Object);
            fileHelperMock.Setup(x => x.GetFileExtensionWithoutDot(It.IsAny<string>())).Returns("csv");
            fileHelperMock.Setup(x => x.GetFileSizeInBytes(It.IsAny<string>())).Returns(5);

            var queueMessage = hubMessageHelper.ConvertRecordToQueueMessage(record);
            var actualObject = JObject.Parse(queueMessage);

            var expectedObject = JObject.Parse(
                @"{
                    ""verb"":""upsert"",
                    ""index"":""topcatdev"",
                    ""document"":{
                        ""id"":""0545c14b-e7fd-472d-8575-5bb75034945f"",
                        ""site"":""datahub"",
                        ""title"":""Test record"",
                        ""content"":""This is a test record"",
                        ""url"":""http://hub.jncc.gov.uk/assets/0545c14b-e7fd-472d-8575-5bb75034945f"",
                        ""resource_type"":""publication"",
                        ""keywords"":[
                            {""value"":""Vocabless record"",""vocab"":""""},
                            {""value"":""Terrestrial"",""vocab"":""http://vocab.jncc.gov.uk/jncc-domain""},
                            {""value"":""Example Collection"",""vocab"":""http://vocab.jncc.gov.uk/jncc-category""}
                        ],
                        ""published_date"":""2015-04-14""
                    },
                    ""resources"":[
                        {
                            ""title"":""A csv resource"",
                            ""content"":""This is a test record"",
                            ""url"":""http://data.jncc.gov.uk/data/0545c14b-e7fd-472d-8575-5bb75034945f/test.csv"",
                            ""file_extension"":""csv"",
                            ""file_bytes"":5
                        }
                    ]
                }");

            JToken.DeepEquals(expectedObject, actualObject).Should().BeTrue();
        }

        [Test]
        public void queue_message_with_one_pdf_resource()
        {
            var record = readyToUploadRecord.With(r => r.Publication.Data = new DataInfo
            {
                Resources = new List<Resource> {
                    new Resource {
                        Name = "A pdf resource",
                        Path = "C:\\work\\test.pdf",
                        PublishedUrl = "http://data.jncc.gov.uk/data/0545c14b-e7fd-472d-8575-5bb75034945f/test.pdf"
                    }
                }
            });

            var fileHelperMock = new Mock<IFileHelper>();
            var hubMessageHelper = new HubMessageConverter(env, fileHelperMock.Object);
            fileHelperMock.Setup(x => x.GetBase64String(It.IsAny<string>())).Returns("encoded file contents");
            fileHelperMock.Setup(x => x.GetFileExtensionWithoutDot(It.IsAny<string>())).Returns("pdf");
            fileHelperMock.Setup(x => x.GetFileSizeInBytes(It.IsAny<string>())).Returns(5);

            var queueMessage = hubMessageHelper.ConvertRecordToQueueMessage(record);
            var actualObject = JObject.Parse(queueMessage);
            
            var expectedObject = JObject.Parse(
                @"{
                    ""verb"":""upsert"",
                    ""index"":""topcatdev"",
                    ""document"":{
                        ""id"":""0545c14b-e7fd-472d-8575-5bb75034945f"",
                        ""site"":""datahub"",
                        ""title"":""Test record"",
                        ""content"":""This is a test record"",
                        ""url"":""http://hub.jncc.gov.uk/assets/0545c14b-e7fd-472d-8575-5bb75034945f"",
                        ""resource_type"":""dataset"",
                        ""keywords"":[
                            {""value"":""Vocabless record"",""vocab"":""""},
                            {""value"":""Terrestrial"",""vocab"":""http://vocab.jncc.gov.uk/jncc-domain""},
                            {""value"":""Example Collection"",""vocab"":""http://vocab.jncc.gov.uk/jncc-category""}
                        ],
                        ""published_date"":""2015-04-14""
                    },
                    ""resources"":[
                        {
                            ""title"":""A pdf resource"",
                            ""content"":""This is a test record"",
                            ""url"":""http://data.jncc.gov.uk/data/0545c14b-e7fd-472d-8575-5bb75034945f/test.pdf"",
                            ""file_base64"":""encoded file contents"",
                            ""file_extension"":""pdf"",
                            ""file_bytes"":5
                        }
                    ]
                }");

            JToken.DeepEquals(expectedObject, actualObject).Should().BeTrue();
        }

        [Test]
        public void queue_message_with_one_url_resource()
        {
            var record = readyToUploadRecord.With(r => r.Publication.Data = new DataInfo
            {
                Resources = new List<Resource> {
                    new Resource {
                        Name = "A url resource",
                        Path = "http://example.url.resource.com"
                    }
                }
            });

            var fileHelperMock = new Mock<IFileHelper>();
            var hubMessageHelper = new HubMessageConverter(env, fileHelperMock.Object);

            var queueMessage = hubMessageHelper.ConvertRecordToQueueMessage(record);

            var actualObject = JObject.Parse(queueMessage);

            var expectedObject = JObject.Parse(
                @"{
                    ""verb"":""upsert"",
                    ""index"":""topcatdev"",
                    ""document"":{
                        ""id"":""0545c14b-e7fd-472d-8575-5bb75034945f"",
                        ""site"":""datahub"",
                        ""title"":""Test record"",
                        ""content"":""This is a test record"",
                        ""url"":""http://hub.jncc.gov.uk/assets/0545c14b-e7fd-472d-8575-5bb75034945f"",
                        ""resource_type"":""dataset"",
                        ""keywords"":[
                            {""value"":""Vocabless record"",""vocab"":""""},
                            {""value"":""Terrestrial"",""vocab"":""http://vocab.jncc.gov.uk/jncc-domain""},
                            {""value"":""Example Collection"",""vocab"":""http://vocab.jncc.gov.uk/jncc-category""}
                        ],
                        ""published_date"":""2015-04-14""
                    },
                    ""resources"":[]
                }");

            JToken.DeepEquals(expectedObject, actualObject).Should().BeTrue();
        }

        [Test]
        public void queue_message_with_multiple_resources()
        {
            var record = readyToUploadRecord.With(r => r.Publication.Data = new DataInfo
            {
                Resources = new List<Resource> {
                    new Resource {
                        Name = "A csv resource",
                        Path = "C:\\work\\test.csv",
                        PublishedUrl = "http://data.jncc.gov.uk/data/0545c14b-e7fd-472d-8575-5bb75034945f/test.csv"
                    },
                    new Resource {
                        Name = "A pdf resource",
                        Path = "C:\\work\\test.pdf",
                        PublishedUrl = "http://data.jncc.gov.uk/data/0545c14b-e7fd-472d-8575-5bb75034945f/test.pdf"
                    },
                    new Resource {
                        Name = "A url resource",
                        Path = "http://example.url.resource.com"
                    }
                }
            });

            var fileHelperMock = new Mock<IFileHelper>();
            var hubMessageHelper = new HubMessageConverter(env, fileHelperMock.Object);
            fileHelperMock.Setup(x => x.GetBase64String(It.IsAny<string>())).Returns("encoded file contents");
            fileHelperMock.Setup(x => x.GetFileExtensionWithoutDot("C:\\work\\test.csv")).Returns("csv");
            fileHelperMock.Setup(x => x.GetFileExtensionWithoutDot("C:\\work\\test.pdf")).Returns("pdf");
            fileHelperMock.Setup(x => x.GetFileSizeInBytes(It.IsAny<string>())).Returns(5);

            var queueMessage = hubMessageHelper.ConvertRecordToQueueMessage(record);
            var actualObject = JObject.Parse(queueMessage);

            var expectedObject = JObject.Parse(
                @"{
                    ""verb"":""upsert"",
                    ""index"":""topcatdev"",
                    ""document"":{
                        ""id"":""0545c14b-e7fd-472d-8575-5bb75034945f"",
                        ""site"":""datahub"",
                        ""title"":""Test record"",
                        ""content"":""This is a test record"",
                        ""url"":""http://hub.jncc.gov.uk/assets/0545c14b-e7fd-472d-8575-5bb75034945f"",
                        ""resource_type"":""dataset"",
                        ""keywords"":[
                            {""value"":""Vocabless record"",""vocab"":""""},
                            {""value"":""Terrestrial"",""vocab"":""http://vocab.jncc.gov.uk/jncc-domain""},
                            {""value"":""Example Collection"",""vocab"":""http://vocab.jncc.gov.uk/jncc-category""}
                        ],
                        ""published_date"":""2015-04-14""
                    },
                    ""resources"":[
                        {
                            ""title"":""A csv resource"",
                            ""content"":""This is a test record"",
                            ""url"":""http://data.jncc.gov.uk/data/0545c14b-e7fd-472d-8575-5bb75034945f/test.csv"",
                            ""file_extension"":""csv"",
                            ""file_bytes"":5
                        },
                        {
                            ""title"":""A pdf resource"",
                            ""content"":""This is a test record"",
                            ""url"":""http://data.jncc.gov.uk/data/0545c14b-e7fd-472d-8575-5bb75034945f/test.pdf"",
                            ""file_base64"":""encoded file contents"",
                            ""file_extension"":""pdf"",
                            ""file_bytes"":5
                        }
                    ]
                }");

            JToken.DeepEquals(expectedObject, actualObject).Should().BeTrue();
        }

        [Test]
        public void queue_message_with_file_resource_but_no_published_url()
        {
            // this might happen if a publish is triggered with the metadata only flag for a record being published for the first time

            var record = readyToUploadRecord.With(r => r.Publication.Data = new DataInfo
            {
                Resources = new List<Resource> {
                    new Resource {
                        Name = "A pdf resource",
                        Path = "C:\\work\\test.pdf"
                    }
                }
            });

            var fileHelperMock = new Mock<IFileHelper>();
            var hubMessageHelper = new HubMessageConverter(env, fileHelperMock.Object);

            var queueMessage = hubMessageHelper.ConvertRecordToQueueMessage(record);
            var actualObject = JObject.Parse(queueMessage);

            var expectedObject = JObject.Parse(
                @"{
                    ""verb"":""upsert"",
                    ""index"":""topcatdev"",
                    ""document"":{
                        ""id"":""0545c14b-e7fd-472d-8575-5bb75034945f"",
                        ""site"":""datahub"",
                        ""title"":""Test record"",
                        ""content"":""This is a test record"",
                        ""url"":""http://hub.jncc.gov.uk/assets/0545c14b-e7fd-472d-8575-5bb75034945f"",
                        ""resource_type"":""dataset"",
                        ""keywords"":[
                            {""value"":""Vocabless record"",""vocab"":""""},
                            {""value"":""Terrestrial"",""vocab"":""http://vocab.jncc.gov.uk/jncc-domain""},
                            {""value"":""Example Collection"",""vocab"":""http://vocab.jncc.gov.uk/jncc-category""}
                        ],
                        ""published_date"":""2015-04-14""
                    },
                    ""resources"":[]
                }");

            JToken.DeepEquals(expectedObject, actualObject).Should().BeTrue();
        }

        [Test]
        public void asset_message_with_no_resources()
        {
            var record = readyToUploadRecord.With(r => r.Publication.Data = null);
            var fileHelperMock = new Mock<IFileHelper>();
            var hubMessageHelper = new HubMessageConverter(env, fileHelperMock.Object);

            var assetMessage = hubMessageHelper.ConvertRecordToHubAsset(record);
            var actualObject = JObject.Parse(assetMessage);
            
            var expectedObject = JObject.Parse(
                @"{
                    ""id"":""0545c14b-e7fd-472d-8575-5bb75034945f"",
                    ""digitalObjectIdentifier"":null,
                    ""citation"":null,
                    ""image"":null,
                    ""metadata"":{
                        ""title"":""Test record"",
                        ""abstract"":""This is a test record"",
                        ""topicCategory"":""geoscientificInformation"",
                        ""keywords"":[
                            {""value"":""Vocabless record"",""vocab"":null},
                            {""value"":""Terrestrial"",""vocab"":""http://vocab.jncc.gov.uk/jncc-domain""},
                            {""value"":""Example Collection"",""vocab"":""http://vocab.jncc.gov.uk/jncc-category""}
                        ],
                        ""temporalExtent"":{
                            ""begin"":""1998-01"",
                            ""end"":null
                        },
                        ""datasetReferenceDate"":""2015-04-14"",
                        ""lineage"":""This dataset was imagined by a developer."",
                        ""dataFormat"":""XLS"",
                        ""responsibleOrganisation"":{
                            ""name"":""Joint Nature Conservation Committee (JNCC)"",
                            ""email"":""data@jncc.gov.uk"",
                            ""role"":""owner""
                        },
                        ""limitationsOnPublicAccess"":""no limitations"",
                        ""useConstraints"":""no conditions apply"",
                        ""spatialReferenceSystem"":""http://www.opengis.net/def/crs/EPSG/0/4326"",
                        ""metadataDate"":""2017-09-26T00:00:00"",
                        ""metadataPointOfContact"":{
                            ""name"":""Joint Nature Conservation Committee (JNCC)"",
                            ""email"":""some.user@jncc.gov.uk"",
                            ""role"":""pointOfContact""
                        },
                        ""resourceType"":""dataset"",
                        ""metadataLanguage"":""English"",
                        ""boundingBox"":{
                            ""north"":60.77,
                            ""south"":49.79,
                            ""east"":2.96,
                            ""west"":-8.14
                        }
                    },
                    ""data"":[]
                }"
            );

            JToken.DeepEquals(expectedObject, actualObject).Should().BeTrue();
        }

        [Test]
        public void asset_message_with_a_url_resource()
        {
            var record = readyToUploadRecord.With(r => r.Publication.Data = new DataInfo
            {
                Resources = new List<Resource> {
                    new Resource {
                        Name = "A url resource",
                        Path = "http://example.url.resource.com"
                    }
                }
            });

            var fileHelperMock = new Mock<IFileHelper>();
            var hubMessageHelper = new HubMessageConverter(env, fileHelperMock.Object);

            var assetMessage = hubMessageHelper.ConvertRecordToHubAsset(record);
            var actualObject = JObject.Parse(assetMessage);
            
            var expectedObject = JObject.Parse(
                @"{
                    ""id"":""0545c14b-e7fd-472d-8575-5bb75034945f"",
                    ""digitalObjectIdentifier"":null,
                    ""citation"":null,
                    ""image"":null,
                    ""metadata"":{
                        ""title"":""Test record"",
                        ""abstract"":""This is a test record"",
                        ""topicCategory"":""geoscientificInformation"",
                        ""keywords"":[
                            {""value"":""Vocabless record"",""vocab"":null},
                            {""value"":""Terrestrial"",""vocab"":""http://vocab.jncc.gov.uk/jncc-domain""},
                            {""value"":""Example Collection"",""vocab"":""http://vocab.jncc.gov.uk/jncc-category""}
                        ],
                        ""temporalExtent"":{
                            ""begin"":""1998-01"",
                            ""end"":null
                        },
                        ""datasetReferenceDate"":""2015-04-14"",
                        ""lineage"":""This dataset was imagined by a developer."",
                        ""dataFormat"":""XLS"",
                        ""responsibleOrganisation"":{
                            ""name"":""Joint Nature Conservation Committee (JNCC)"",
                            ""email"":""data@jncc.gov.uk"",
                            ""role"":""owner""
                        },
                        ""limitationsOnPublicAccess"":""no limitations"",
                        ""useConstraints"":""no conditions apply"",
                        ""spatialReferenceSystem"":""http://www.opengis.net/def/crs/EPSG/0/4326"",
                        ""metadataDate"":""2017-09-26T00:00:00"",
                        ""metadataPointOfContact"":{
                            ""name"":""Joint Nature Conservation Committee (JNCC)"",
                            ""email"":""some.user@jncc.gov.uk"",
                            ""role"":""pointOfContact""
                        },
                        ""resourceType"":""dataset"",
                        ""metadataLanguage"":""English"",
                        ""boundingBox"":{
                            ""north"":60.77,
                            ""south"":49.79,
                            ""east"":2.96,
                            ""west"":-8.14
                        }
                    },
                    ""data"":[
                        {
                            ""title"":""A url resource"",
                            ""http"":{
                                ""url"":""http://example.url.resource.com""
                            }
                        }
                    ]
                }"
            );

            JToken.DeepEquals(expectedObject, actualObject).Should().BeTrue();
        }

        [Test]
        public void asset_message_with_a_file_resource()
        {
            var record = readyToUploadRecord.With(r => r.Publication.Data = new DataInfo
            {
                Resources = new List<Resource> {
                    new Resource {
                        Name = "A pdf resource",
                        Path = "C:\\work\\test.pdf",
                        PublishedUrl = "http://data.jncc.gov.uk/data/0545c14b-e7fd-472d-8575-5bb75034945f/test.pdf"
                    },
                }
            });

            var fileHelperMock = new Mock<IFileHelper>();
            var hubMessageHelper = new HubMessageConverter(env, fileHelperMock.Object);
            fileHelperMock.Setup(x => x.GetFileExtensionWithoutDot("C:\\work\\test.pdf")).Returns("pdf");
            fileHelperMock.Setup(x => x.GetFileSizeInBytes(It.IsAny<string>())).Returns(5);

            var assetMessage = hubMessageHelper.ConvertRecordToHubAsset(record);
            var actualObject = JObject.Parse(assetMessage);
            
            var expectedObject = JObject.Parse(
                @"{
                    ""id"":""0545c14b-e7fd-472d-8575-5bb75034945f"",
                    ""digitalObjectIdentifier"":null,
                    ""citation"":null,
                    ""image"":null,
                    ""metadata"":{
                        ""title"":""Test record"",
                        ""abstract"":""This is a test record"",
                        ""topicCategory"":""geoscientificInformation"",
                        ""keywords"":[
                            {""value"":""Vocabless record"",""vocab"":null},
                            {""value"":""Terrestrial"",""vocab"":""http://vocab.jncc.gov.uk/jncc-domain""},
                            {""value"":""Example Collection"",""vocab"":""http://vocab.jncc.gov.uk/jncc-category""}
                        ],
                        ""temporalExtent"":{
                            ""begin"":""1998-01"",
                            ""end"":null
                        },
                        ""datasetReferenceDate"":""2015-04-14"",
                        ""lineage"":""This dataset was imagined by a developer."",
                        ""dataFormat"":""XLS"",
                        ""responsibleOrganisation"":{
                            ""name"":""Joint Nature Conservation Committee (JNCC)"",
                            ""email"":""data@jncc.gov.uk"",
                            ""role"":""owner""
                        },
                        ""limitationsOnPublicAccess"":""no limitations"",
                        ""useConstraints"":""no conditions apply"",
                        ""spatialReferenceSystem"":""http://www.opengis.net/def/crs/EPSG/0/4326"",
                        ""metadataDate"":""2017-09-26T00:00:00"",
                        ""metadataPointOfContact"":{
                            ""name"":""Joint Nature Conservation Committee (JNCC)"",
                            ""email"":""some.user@jncc.gov.uk"",
                            ""role"":""pointOfContact""
                        },
                        ""resourceType"":""dataset"",
                        ""metadataLanguage"":""English"",
                        ""boundingBox"":{
                            ""north"":60.77,
                            ""south"":49.79,
                            ""east"":2.96,
                            ""west"":-8.14
                        }
                    },
                    ""data"":[
                        {
                            ""title"":""A pdf resource"",
                            ""http"":{
                                ""url"":""http://data.jncc.gov.uk/data/0545c14b-e7fd-472d-8575-5bb75034945f/test.pdf"",
                                ""fileExtension"":""pdf"",
                                ""fileBytes"":5
                            }
                        }
                    ]
                }"
            );

            JToken.DeepEquals(expectedObject, actualObject).Should().BeTrue();
        }

        [Test]
        public void asset_message_with_multiple_resources()
        {
            var record = readyToUploadRecord.With(r => r.Publication.Data = new DataInfo
            {
                Resources = new List<Resource> {
                    new Resource {
                        Name = "A pdf resource",
                        Path = "C:\\work\\test.pdf",
                        PublishedUrl = "http://data.jncc.gov.uk/data/0545c14b-e7fd-472d-8575-5bb75034945f/test.pdf"
                    },
                    new Resource {
                        Name = "A url resource",
                        Path = "http://example.url.resource.com"
                    }
                }
            });

            var fileHelperMock = new Mock<IFileHelper>();
            var hubMessageHelper = new HubMessageConverter(env, fileHelperMock.Object);
            fileHelperMock.Setup(x => x.GetFileExtensionWithoutDot("C:\\work\\test.pdf")).Returns("pdf");
            fileHelperMock.Setup(x => x.GetFileSizeInBytes(It.IsAny<string>())).Returns(5);

            var assetMessage = hubMessageHelper.ConvertRecordToHubAsset(record);
            var actualObject = JObject.Parse(assetMessage);

            var expectedObject = JObject.Parse(
                @"{
                    ""id"":""0545c14b-e7fd-472d-8575-5bb75034945f"",
                    ""digitalObjectIdentifier"":null,
                    ""citation"":null,
                    ""image"":null,
                    ""metadata"":{
                        ""title"":""Test record"",
                        ""abstract"":""This is a test record"",
                        ""topicCategory"":""geoscientificInformation"",
                        ""keywords"":[
                            {""value"":""Vocabless record"",""vocab"":null},
                            {""value"":""Terrestrial"",""vocab"":""http://vocab.jncc.gov.uk/jncc-domain""},
                            {""value"":""Example Collection"",""vocab"":""http://vocab.jncc.gov.uk/jncc-category""}
                        ],
                        ""temporalExtent"":{
                            ""begin"":""1998-01"",
                            ""end"":null
                        },
                        ""datasetReferenceDate"":""2015-04-14"",
                        ""lineage"":""This dataset was imagined by a developer."",
                        ""dataFormat"":""XLS"",
                        ""responsibleOrganisation"":{
                            ""name"":""Joint Nature Conservation Committee (JNCC)"",
                            ""email"":""data@jncc.gov.uk"",
                            ""role"":""owner""
                        },
                        ""limitationsOnPublicAccess"":""no limitations"",
                        ""useConstraints"":""no conditions apply"",
                        ""spatialReferenceSystem"":""http://www.opengis.net/def/crs/EPSG/0/4326"",
                        ""metadataDate"":""2017-09-26T00:00:00"",
                        ""metadataPointOfContact"":{
                            ""name"":""Joint Nature Conservation Committee (JNCC)"",
                            ""email"":""some.user@jncc.gov.uk"",
                            ""role"":""pointOfContact""
                        },
                        ""resourceType"":""dataset"",
                        ""metadataLanguage"":""English"",
                        ""boundingBox"":{
                            ""north"":60.77,
                            ""south"":49.79,
                            ""east"":2.96,
                            ""west"":-8.14
                        }
                    },
                    ""data"":[
                        {
                            ""title"":""A pdf resource"",
                            ""http"":{
                                ""url"":""http://data.jncc.gov.uk/data/0545c14b-e7fd-472d-8575-5bb75034945f/test.pdf"",
                                ""fileExtension"":""pdf"",
                                ""fileBytes"":5
                            }
                        },
                        {
                            ""title"":""A url resource"",
                            ""http"":{
                                ""url"":""http://example.url.resource.com""
                            }
                        }
                    ]
                }"
            );

            JToken.DeepEquals(expectedObject, actualObject).Should().BeTrue();
        }

        [Test]
        public void asset_message_with_file_resource_but_no_published_url()
        {
            // this might happen if a publish is triggered with the metadata only flag for a record being published for the first time

            var record = readyToUploadRecord.With(r => r.Publication.Data = new DataInfo
            {
                Resources = new List<Resource> {
                    new Resource {
                        Name = "A pdf resource",
                        Path = "C:\\work\\test.pdf"
                    }
                }
            });

            var fileHelperMock = new Mock<IFileHelper>();
            var hubMessageHelper = new HubMessageConverter(env, fileHelperMock.Object);

            var assetMessage = hubMessageHelper.ConvertRecordToHubAsset(record);
            var actualObject = JObject.Parse(assetMessage);

            Console.Out.WriteLine(assetMessage);

            var expectedObject = JObject.Parse(
                @"{
                    ""id"":""0545c14b-e7fd-472d-8575-5bb75034945f"",
                    ""digitalObjectIdentifier"":null,
                    ""citation"":null,
                    ""image"":null,
                    ""metadata"":{
                        ""title"":""Test record"",
                        ""abstract"":""This is a test record"",
                        ""topicCategory"":""geoscientificInformation"",
                        ""keywords"":[
                            {""value"":""Vocabless record"",""vocab"":null},
                            {""value"":""Terrestrial"",""vocab"":""http://vocab.jncc.gov.uk/jncc-domain""},
                            {""value"":""Example Collection"",""vocab"":""http://vocab.jncc.gov.uk/jncc-category""}
                        ],
                        ""temporalExtent"":{
                            ""begin"":""1998-01"",
                            ""end"":null
                        },
                        ""datasetReferenceDate"":""2015-04-14"",
                        ""lineage"":""This dataset was imagined by a developer."",
                        ""dataFormat"":""XLS"",
                        ""responsibleOrganisation"":{
                            ""name"":""Joint Nature Conservation Committee (JNCC)"",
                            ""email"":""data@jncc.gov.uk"",
                            ""role"":""owner""
                        },
                        ""limitationsOnPublicAccess"":""no limitations"",
                        ""useConstraints"":""no conditions apply"",
                        ""spatialReferenceSystem"":""http://www.opengis.net/def/crs/EPSG/0/4326"",
                        ""metadataDate"":""2017-09-26T00:00:00"",
                        ""metadataPointOfContact"":{
                            ""name"":""Joint Nature Conservation Committee (JNCC)"",
                            ""email"":""some.user@jncc.gov.uk"",
                            ""role"":""pointOfContact""
                        },
                        ""resourceType"":""dataset"",
                        ""metadataLanguage"":""English"",
                        ""boundingBox"":{
                            ""north"":60.77,
                            ""south"":49.79,
                            ""east"":2.96,
                            ""west"":-8.14
                        }
                    },
                    ""data"":[]
                }"
            );

            JToken.DeepEquals(expectedObject, actualObject).Should().BeTrue();
        }

        [Test]
        public void asset_message_with_doi_and_citation()
        {
            var record = readyToUploadRecord.With(r =>
            {
                r.DigitalObjectIdentifier = "10.25603/123456.0.0.0";
                r.Citation = "a citation";
                r.Publication.Data = new DataInfo
                {
                    Resources = new List<Resource>
                    {
                        new Resource
                        {
                            Name = "A pdf resource",
                            Path = "C:\\work\\test.pdf",
                            PublishedUrl = "http://data.jncc.gov.uk/data/0545c14b-e7fd-472d-8575-5bb75034945f/test.pdf"
                        },
                        new Resource
                        {
                            Name = "A url resource",
                            Path = "http://example.url.resource.com"
                        }
                    }
                };
            });

            var fileHelperMock = new Mock<IFileHelper>();
            var hubMessageHelper = new HubMessageConverter(env, fileHelperMock.Object);
            fileHelperMock.Setup(x => x.GetFileExtensionWithoutDot("C:\\work\\test.pdf")).Returns("pdf");
            fileHelperMock.Setup(x => x.GetFileSizeInBytes(It.IsAny<string>())).Returns(5);

            var assetMessage = hubMessageHelper.ConvertRecordToHubAsset(record);
            var actualObject = JObject.Parse(assetMessage);

            var expectedObject = JObject.Parse(
                @"{
                    ""id"":""0545c14b-e7fd-472d-8575-5bb75034945f"",
                    ""digitalObjectIdentifier"":""10.25603/123456.0.0.0"",
                    ""citation"":""a citation"",
                    ""image"":null,
                    ""metadata"":{
                        ""title"":""Test record"",
                        ""abstract"":""This is a test record"",
                        ""topicCategory"":""geoscientificInformation"",
                        ""keywords"":[
                            {""value"":""Vocabless record"",""vocab"":null},
                            {""value"":""Terrestrial"",""vocab"":""http://vocab.jncc.gov.uk/jncc-domain""},
                            {""value"":""Example Collection"",""vocab"":""http://vocab.jncc.gov.uk/jncc-category""}
                        ],
                        ""temporalExtent"":{
                            ""begin"":""1998-01"",
                            ""end"":null
                        },
                        ""datasetReferenceDate"":""2015-04-14"",
                        ""lineage"":""This dataset was imagined by a developer."",
                        ""dataFormat"":""XLS"",
                        ""responsibleOrganisation"":{
                            ""name"":""Joint Nature Conservation Committee (JNCC)"",
                            ""email"":""data@jncc.gov.uk"",
                            ""role"":""owner""
                        },
                        ""limitationsOnPublicAccess"":""no limitations"",
                        ""useConstraints"":""no conditions apply"",
                        ""spatialReferenceSystem"":""http://www.opengis.net/def/crs/EPSG/0/4326"",
                        ""metadataDate"":""2017-09-26T00:00:00"",
                        ""metadataPointOfContact"":{
                            ""name"":""Joint Nature Conservation Committee (JNCC)"",
                            ""email"":""some.user@jncc.gov.uk"",
                            ""role"":""pointOfContact""
                        },
                        ""resourceType"":""dataset"",
                        ""metadataLanguage"":""English"",
                        ""boundingBox"":{
                            ""north"":60.77,
                            ""south"":49.79,
                            ""east"":2.96,
                            ""west"":-8.14
                        }
                    },
                    ""data"":[
                        {
                            ""title"":""A pdf resource"",
                            ""http"":{
                                ""url"":""http://data.jncc.gov.uk/data/0545c14b-e7fd-472d-8575-5bb75034945f/test.pdf"",
                                ""fileExtension"":""pdf"",
                                ""fileBytes"":5
                            }
                        },
                        {
                            ""title"":""A url resource"",
                            ""http"":{
                                ""url"":""http://example.url.resource.com""
                            }
                        }
                    ]
                }"
            );

            JToken.DeepEquals(expectedObject, actualObject).Should().BeTrue();
        }

        [Test]
        public void asset_message_with_empty_strings()
        {

            var record = readyToUploadRecord.With(r =>
            {
                r.Gemini.ResourceType = "publication";
                r.Gemini.Keywords.Add(new MetadataKeyword { Value = "test keyword", Vocab = "" });
                r.Gemini.TemporalExtent = new TemporalExtent
                {
                    Begin = "",
                    End = ""
                };
                r.Gemini.SpatialReferenceSystem = "";
                r.Gemini.DataFormat = "";
                r.DigitalObjectIdentifier = "";
                r.Citation = "";
                r.Publication.Data = new DataInfo
                {
                    Resources = new List<Resource>
                    {
                        new Resource
                        {
                            Name = "A pdf resource",
                            Path = "C:\\work\\test.pdf",
                            PublishedUrl = "http://data.jncc.gov.uk/data/0545c14b-e7fd-472d-8575-5bb75034945f/test.pdf"
                        },
                        new Resource
                        {
                            Name = "A url resource",
                            Path = "http://example.url.resource.com"
                        }
                    }
                };
            });

            var fileHelperMock = new Mock<IFileHelper>();
            var hubMessageHelper = new HubMessageConverter(env, fileHelperMock.Object);
            fileHelperMock.Setup(x => x.GetFileExtensionWithoutDot("C:\\work\\test.pdf")).Returns("pdf");
            fileHelperMock.Setup(x => x.GetFileSizeInBytes(It.IsAny<string>())).Returns(5);

            var assetMessage = hubMessageHelper.ConvertRecordToHubAsset(record);
            var actualObject = JObject.Parse(assetMessage);

            var expectedObject = JObject.Parse(
                @"{
                    ""id"":""0545c14b-e7fd-472d-8575-5bb75034945f"",
                    ""digitalObjectIdentifier"":null,
                    ""citation"":null,
                    ""image"":null,
                    ""metadata"":{
                        ""title"":""Test record"",
                        ""abstract"":""This is a test record"",
                        ""topicCategory"":""geoscientificInformation"",
                        ""keywords"":[
                            {""value"":""Vocabless record"",""vocab"":null},
                            {""value"":""Terrestrial"",""vocab"":""http://vocab.jncc.gov.uk/jncc-domain""},
                            {""value"":""Example Collection"",""vocab"":""http://vocab.jncc.gov.uk/jncc-category""},
                            {""value"":""test keyword"",""vocab"":null}
                        ],
                        ""temporalExtent"":{
                            ""begin"":null,
                            ""end"":null
                        },
                        ""datasetReferenceDate"":""2015-04-14"",
                        ""lineage"":""This dataset was imagined by a developer."",
                        ""dataFormat"":null,
                        ""responsibleOrganisation"":{
                            ""name"":""Joint Nature Conservation Committee (JNCC)"",
                            ""email"":""data@jncc.gov.uk"",
                            ""role"":""owner""
                        },
                        ""limitationsOnPublicAccess"":""no limitations"",
                        ""useConstraints"":""no conditions apply"",
                        ""spatialReferenceSystem"":null,
                        ""metadataDate"":""2017-09-26T00:00:00"",
                        ""metadataPointOfContact"":{
                            ""name"":""Joint Nature Conservation Committee (JNCC)"",
                            ""email"":""some.user@jncc.gov.uk"",
                            ""role"":""pointOfContact""
                        },
                        ""resourceType"":""publication"",
                        ""metadataLanguage"":""English"",
                        ""boundingBox"":{
                            ""north"":60.77,
                            ""south"":49.79,
                            ""east"":2.96,
                            ""west"":-8.14
                        }
                    },
                    ""data"":[
                        {
                            ""title"":""A pdf resource"",
                            ""http"":{
                                ""url"":""http://data.jncc.gov.uk/data/0545c14b-e7fd-472d-8575-5bb75034945f/test.pdf"",
                                ""fileExtension"":""pdf"",
                                ""fileBytes"":5
                            }
                        },
                        {
                            ""title"":""A url resource"",
                            ""http"":{
                                ""url"":""http://example.url.resource.com""
                            }
                        }
                    ]
                }"
            );

            JToken.DeepEquals(expectedObject, actualObject).Should().BeTrue();
        }

        [Test]
        public void asset_message_with_website_image()
        {
            var record = readyToUploadRecord.With(r =>
            {
                r.Image = new Image
                {
                    Url = "http://jncc.defra.gov.uk/laf/JNCCLogo.png",
                    Height = 72,
                    Width = 199,
                    Crops = new ImageCrops
                    {
                        SquareUrl = "http://jncc.defra.gov.uk/laf/JNCCLogo.png",
                        ThumbnailUrl = "http://jncc.defra.gov.uk/laf/JNCCLogo.png"
                    }
                };
                r.Publication.Data = new DataInfo
                {
                    Resources = new List<Resource>
                    {
                        new Resource
                        {
                            Name = "A pdf resource",
                            Path = "C:\\work\\test.pdf",
                            PublishedUrl = "http://data.jncc.gov.uk/data/0545c14b-e7fd-472d-8575-5bb75034945f/test.pdf"
                        },
                        new Resource
                        {
                            Name = "A url resource",
                            Path = "http://example.url.resource.com"
                        }
                    }
                };
            });

            var fileHelperMock = new Mock<IFileHelper>();
            var hubMessageHelper = new HubMessageConverter(env, fileHelperMock.Object);
            fileHelperMock.Setup(x => x.GetFileExtensionWithoutDot("C:\\work\\test.pdf")).Returns("pdf");
            fileHelperMock.Setup(x => x.GetFileSizeInBytes(It.IsAny<string>())).Returns(5);

            var assetMessage = hubMessageHelper.ConvertRecordToHubAsset(record);
            var actualObject = JObject.Parse(assetMessage);

            var expectedObject = JObject.Parse(
                @"{
                    ""id"":""0545c14b-e7fd-472d-8575-5bb75034945f"",
                    ""digitalObjectIdentifier"":null,
                    ""citation"":null,
                    ""image"":{
                        ""url"":""http://jncc.defra.gov.uk/laf/JNCCLogo.png"",
                        ""height"":72,
                        ""width"":199,
                        ""crops"":{
                            ""squareUrl"":""http://jncc.defra.gov.uk/laf/JNCCLogo.png"",
                            ""thumbnailUrl"":""http://jncc.defra.gov.uk/laf/JNCCLogo.png""
                        }
                    },
                    ""metadata"":{
                        ""title"":""Test record"",
                        ""abstract"":""This is a test record"",
                        ""topicCategory"":""geoscientificInformation"",
                        ""keywords"":[
                            {""value"":""Vocabless record"",""vocab"":null},
                            {""value"":""Terrestrial"",""vocab"":""http://vocab.jncc.gov.uk/jncc-domain""},
                            {""value"":""Example Collection"",""vocab"":""http://vocab.jncc.gov.uk/jncc-category""}
                        ],
                        ""temporalExtent"":{
                            ""begin"":""1998-01"",
                            ""end"":null
                        },
                        ""datasetReferenceDate"":""2015-04-14"",
                        ""lineage"":""This dataset was imagined by a developer."",
                        ""dataFormat"":""XLS"",
                        ""responsibleOrganisation"":{
                            ""name"":""Joint Nature Conservation Committee (JNCC)"",
                            ""email"":""data@jncc.gov.uk"",
                            ""role"":""owner""
                        },
                        ""limitationsOnPublicAccess"":""no limitations"",
                        ""useConstraints"":""no conditions apply"",
                        ""spatialReferenceSystem"":""http://www.opengis.net/def/crs/EPSG/0/4326"",
                        ""metadataDate"":""2017-09-26T00:00:00"",
                        ""metadataPointOfContact"":{
                            ""name"":""Joint Nature Conservation Committee (JNCC)"",
                            ""email"":""some.user@jncc.gov.uk"",
                            ""role"":""pointOfContact""
                        },
                        ""resourceType"":""dataset"",
                        ""metadataLanguage"":""English"",
                        ""boundingBox"":{
                            ""north"":60.77,
                            ""south"":49.79,
                            ""east"":2.96,
                            ""west"":-8.14
                        }
                    },
                    ""data"":[
                        {
                            ""title"":""A pdf resource"",
                            ""http"":{
                                ""url"":""http://data.jncc.gov.uk/data/0545c14b-e7fd-472d-8575-5bb75034945f/test.pdf"",
                                ""fileExtension"":""pdf"",
                                ""fileBytes"":5
                            }
                        },
                        {
                            ""title"":""A url resource"",
                            ""http"":{
                                ""url"":""http://example.url.resource.com""
                            }
                        }
                    ]
                }"
            );

            JToken.DeepEquals(expectedObject, actualObject).Should().BeTrue();
        }

        [Test]
        public void asset_message_with_url_only_image()
        {
            var record = readyToUploadRecord.With(r =>
            {
                r.Image = new Image
                {
                    Url = "http://jncc.defra.gov.uk/laf/JNCCLogo.png",
                    Height = 0,
                    Width = 0,
                    Crops = new ImageCrops
                    {
                        SquareUrl = null,
                        ThumbnailUrl = null
                    }
                };
                r.Publication.Data = new DataInfo
                {
                    Resources = new List<Resource>
                    {
                        new Resource
                        {
                            Name = "A pdf resource",
                            Path = "C:\\work\\test.pdf",
                            PublishedUrl = "http://data.jncc.gov.uk/data/0545c14b-e7fd-472d-8575-5bb75034945f/test.pdf"
                        },
                        new Resource
                        {
                            Name = "A url resource",
                            Path = "http://example.url.resource.com"
                        }
                    }
                };
            });

            var fileHelperMock = new Mock<IFileHelper>();
            var hubMessageHelper = new HubMessageConverter(env, fileHelperMock.Object);
            fileHelperMock.Setup(x => x.GetFileExtensionWithoutDot("C:\\work\\test.pdf")).Returns("pdf");
            fileHelperMock.Setup(x => x.GetFileSizeInBytes(It.IsAny<string>())).Returns(5);

            var assetMessage = hubMessageHelper.ConvertRecordToHubAsset(record);
            var actualObject = JObject.Parse(assetMessage);

            var expectedObject = JObject.Parse(
                @"{
                    ""id"":""0545c14b-e7fd-472d-8575-5bb75034945f"",
                    ""digitalObjectIdentifier"":null,
                    ""citation"":null,
                    ""image"":{
                        ""url"":""http://jncc.defra.gov.uk/laf/JNCCLogo.png"",
                        ""height"":0,
                        ""width"":0,
                        ""crops"":{
                            ""squareUrl"":null,
                            ""thumbnailUrl"":null
                        }
                    },
                    ""metadata"":{
                        ""title"":""Test record"",
                        ""abstract"":""This is a test record"",
                        ""topicCategory"":""geoscientificInformation"",
                        ""keywords"":[
                            {""value"":""Vocabless record"",""vocab"":null},
                            {""value"":""Terrestrial"",""vocab"":""http://vocab.jncc.gov.uk/jncc-domain""},
                            {""value"":""Example Collection"",""vocab"":""http://vocab.jncc.gov.uk/jncc-category""}
                        ],
                        ""temporalExtent"":{
                            ""begin"":""1998-01"",
                            ""end"":null
                        },
                        ""datasetReferenceDate"":""2015-04-14"",
                        ""lineage"":""This dataset was imagined by a developer."",
                        ""dataFormat"":""XLS"",
                        ""responsibleOrganisation"":{
                            ""name"":""Joint Nature Conservation Committee (JNCC)"",
                            ""email"":""data@jncc.gov.uk"",
                            ""role"":""owner""
                        },
                        ""limitationsOnPublicAccess"":""no limitations"",
                        ""useConstraints"":""no conditions apply"",
                        ""spatialReferenceSystem"":""http://www.opengis.net/def/crs/EPSG/0/4326"",
                        ""metadataDate"":""2017-09-26T00:00:00"",
                        ""metadataPointOfContact"":{
                            ""name"":""Joint Nature Conservation Committee (JNCC)"",
                            ""email"":""some.user@jncc.gov.uk"",
                            ""role"":""pointOfContact""
                        },
                        ""resourceType"":""dataset"",
                        ""metadataLanguage"":""English"",
                        ""boundingBox"":{
                            ""north"":60.77,
                            ""south"":49.79,
                            ""east"":2.96,
                            ""west"":-8.14
                        }
                    },
                    ""data"":[
                        {
                            ""title"":""A pdf resource"",
                            ""http"":{
                                ""url"":""http://data.jncc.gov.uk/data/0545c14b-e7fd-472d-8575-5bb75034945f/test.pdf"",
                                ""fileExtension"":""pdf"",
                                ""fileBytes"":5
                            }
                        },
                        {
                            ""title"":""A url resource"",
                            ""http"":{
                                ""url"":""http://example.url.resource.com""
                            }
                        }
                    ]
                }"
            );

            JToken.DeepEquals(expectedObject, actualObject).Should().BeTrue();
        }
    }
}
