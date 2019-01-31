using Catalogue.Data;
using Catalogue.Data.Model;
using Catalogue.Data.Publishing;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Catalogue.Tests.Slow.Catalogue.Data.Publishing
{
    class publishing_policy_specs
    {
        [Test]
        public void open_data_record_with_a_single_resource(
            [Values("dataset", "nonGeographicDataset", "service")] string resourceType,
            [Values(@"z:\a\file\resource.txt", "http://a.url.resource")] string path,
            [Values(null, false)] bool? publishable)
        {
            var resource = new Resource { Name = "A named resource", Path = path };
            var recordId = Helpers.AddCollection(Guid.NewGuid().ToString());
            var record = new Record
            {
                Id = recordId,
                Path = @"X:\some\path",
                Gemini = Library.Example()
                    .With(r => r.ResourceType = resourceType),
                Publication = new PublicationInfo
                {
                    Data = new DataPublicationInfo
                    {
                        Resources = new List<Resource> { resource }
                    },
                    Gov = new GovPublicationInfo
                    {
                        Publishable = publishable
                    }
                }
            };
            var result = PublishingPolicy.GetPublishingPolicyResult(record);

            result.HubRecord.Should().BeTrue();
            result.HubResources.Count.Should().Be(1);
            result.HubResources.Should().BeEquivalentTo(new List<Resource> { resource });
            result.GovRecord.Should().BeTrue();
            result.GovResources.Should().BeNullOrEmpty();
            result.Message.Should().Be("This is an Open Data record.");
        }

        [Test]
        public void open_data_record_with_no_resources(
            [Values("dataset", "nonGeographicDataset", "service")] string resourceType,
            [Values(null, false)] bool? publishable)
        {
            var record = new Record
            {
                Id = Helpers.AddCollection(Guid.NewGuid().ToString()),
                Path = @"X:\some\path",
                Gemini = Library.Example()
                    .With(r => r.ResourceType = resourceType),
                Publication = new PublicationInfo
                {
                    Data = new DataPublicationInfo
                    {
                        Resources = null
                    },
                    Gov = new GovPublicationInfo
                    {
                        Publishable = publishable
                    }
                }
            };
            var result = PublishingPolicy.GetPublishingPolicyResult(record);

            result.HubRecord.Should().BeTrue();
            result.HubResources.Should().BeNullOrEmpty();
            result.GovRecord.Should().BeTrue();
            result.GovResources.Should().BeNullOrEmpty();
            result.Message.Should().Be("This is an Open Data record.");
        }

        [Test]
        public void open_data_record_with_multiple_resources(
            [Values("dataset", "nonGeographicDataset", "service")] string resourceType,
            [Values(null, false)] bool? publishable)
        {
            var fileResource = new Resource { Name = "A file resource", Path = @"z:\a\file\resource.txt" };
            var urlResource1 = new Resource { Name = "A URL resource", Path = "http://a.url.resource" };
            var urlResource2 = new Resource { Name = "Another URL resource", Path = "http://another.url.resource" };
            var recordId = Helpers.AddCollection(Guid.NewGuid().ToString());
            var record = new Record
            {
                Id = recordId,
                Path = @"X:\some\path",
                Gemini = Library.Example()
                    .With(r => r.ResourceType = resourceType),
                Publication = new PublicationInfo
                {
                    Data = new DataPublicationInfo
                    {
                        Resources = new List<Resource> { fileResource, urlResource1, urlResource2 }
                    },
                    Gov = new GovPublicationInfo
                    {
                        Publishable = publishable
                    }
                }
            };
            var result = PublishingPolicy.GetPublishingPolicyResult(record);

            result.HubRecord.Should().BeTrue();
            result.HubResources.Count.Should().Be(3);
            result.HubResources.Should().BeEquivalentTo(new List<Resource> { fileResource, urlResource1, urlResource2 });
            result.GovRecord.Should().BeTrue();
            result.GovResources.Should().BeNullOrEmpty();
            result.Message.Should().Be("This is an Open Data record.");
        }

        [Test]
        public void republish_an_open_data_record(
            [Values("dataset", "nonGeographicDataset", "service")] string resourceType,
            [Values(@"z:\a\file\resource.txt", "http://a.url.resource")] string path,
            [Values(null, false)] bool? publishable)
        {
            var resource = new Resource { Name = "A named resource", Path = path };
            var recordId = Helpers.AddCollection(Guid.NewGuid().ToString());
            var record = new Record
            {
                Id = recordId,
                Path = @"X:\some\path",
                Gemini = Library.Example()
                    .With(r => r.ResourceType = resourceType),
                Publication = new PublicationInfo
                {
                    Data = new DataPublicationInfo
                    {
                        Resources = new List<Resource> { resource }
                    },
                    Gov = new GovPublicationInfo
                    {
                        Publishable = publishable,
                        LastSuccess = new PublicationAttempt { DateUtc = DateTime.Today }
                    }
                }
            };
            var result = PublishingPolicy.GetPublishingPolicyResult(record);

            result.HubRecord.Should().BeTrue();
            result.HubResources.Count.Should().Be(1);
            result.HubResources.Should().BeEquivalentTo(new List<Resource> { resource });
            result.GovRecord.Should().BeTrue();
            result.GovResources.Should().BeNullOrEmpty();
            result.Message.Should().Be("This is an Open Data record.");
        }

        [Test]
        public void publish_a_doi_dataset_for_the_first_time(
            [Values("dataset", "nonGeographicDataset", "service")] string resourceType,
            [Values(@"z:\a\file\resource.txt", "http://a.url.resource")] string path)
        {
            var resource = new Resource { Name = "A named resource", Path = path };
            var recordId = Helpers.AddCollection(Guid.NewGuid().ToString());
            var record = new Record
            {
                Id = recordId,
                Path = @"X:\some\path",
                Gemini = Library.Example()
                    .With(r => r.ResourceType = resourceType),
                Publication = new PublicationInfo
                {
                    Data = new DataPublicationInfo
                    {
                        Resources = new List<Resource> { resource }
                    },
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true
                    }
                },
                Citation = "a citation reference",
                DigitalObjectIdentifier = "10.25603/840424.1.0.0"
            };
            var result = PublishingPolicy.GetPublishingPolicyResult(record);

            result.HubRecord.Should().BeTrue();
            result.HubResources.Count.Should().Be(1);
            result.HubResources.Should().BeEquivalentTo(new List<Resource> { resource });
            result.GovRecord.Should().BeTrue();
            result.GovResources.Should().BeNullOrEmpty();
            result.Message.Should().Be("This is an Open Data record.");
        }

        [Test]
        public void publish_a_doi_publication_for_the_first_time(
            [Values("publication")] string resourceType,
            [Values(@"z:\a\file\resource.txt", "http://a.url.resource")] string path)
        {
            var resource = new Resource { Name = "A named resource", Path = path };
            var recordId = Helpers.AddCollection(Guid.NewGuid().ToString());
            var record = new Record
            {
                Id = recordId,
                Path = @"X:\some\path",
                Gemini = Library.Example()
                    .With(r => r.ResourceType = resourceType),
                Publication = new PublicationInfo
                {
                    Data = new DataPublicationInfo
                    {
                        Resources = new List<Resource> { resource }
                    },
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true
                    }
                },
                Citation = "a citation reference",
                DigitalObjectIdentifier = "10.25603/840424.1.0.0"
            };
            var result = PublishingPolicy.GetPublishingPolicyResult(record);

            result.HubRecord.Should().BeTrue();
            result.HubResources.Count.Should().Be(1);
            result.HubResources.Should().BeEquivalentTo(new List<Resource> { resource });
            result.GovRecord.Should().BeFalse();
            result.GovResources.Should().BeNullOrEmpty();
            result.Message.Should().Be("This is a JNCC publication.");
        }

        [Test]
        public void republish_a_doi_record(
            [Values("publication", "dataset", "nonGeographicDataset", "service")] string resourceType,
            [Values(@"z:\a\file\resource.txt", "http://a.url.resource")] string path)
        {
            var resource = new Resource { Name = "A named resource", Path = path };
            var recordId = Helpers.AddCollection(Guid.NewGuid().ToString());
            var record = new Record
            {
                Id = recordId,
                Path = @"X:\some\path",
                Gemini = Library.Example()
                    .With(r => r.ResourceType = resourceType),
                Publication = new PublicationInfo
                {
                    Data = new DataPublicationInfo
                    {
                        Resources = new List<Resource> { resource }
                    },
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true,
                        LastSuccess = new PublicationAttempt { DateUtc = DateTime.Today }
                    }
                },
                Citation = "a citation reference",
                DigitalObjectIdentifier = "10.25603/840424.1.0.0"
            };
            var result = PublishingPolicy.GetPublishingPolicyResult(record);

            result.HubRecord.Should().BeFalse();
            result.HubResources.Should().BeNullOrEmpty();
            result.GovRecord.Should().BeFalse();
            result.GovResources.Should().BeNullOrEmpty();
            result.Message.Should().Be("This record has a DOI and cannot be republished.");
        }

        [Test]
        public void record_with_a_restrictive_licence(
            [Values("dataset", "nonGeographicDataset", "service")] string resourceType,
            [Values(@"z:\a\file\resource.txt", "http://a.url.resource")] string path)
        {
            var resource = new Resource { Name = "A named resource", Path = path };
            var recordId = Helpers.AddCollection(Guid.NewGuid().ToString());
            var record = new Record
            {
                Id = recordId,
                Path = @"X:\some\path",
                Gemini = Library.Example()
                    .With(r => r.Keywords.Add(new MetadataKeyword { Value = "Restrictive Licence" }))
                    .With(r => r.ResourceType = resourceType),
                Publication = new PublicationInfo
                {
                    Data = new DataPublicationInfo
                    {
                        Resources = new List<Resource> { resource }
                    },
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true
                    }
                }
            };
            var result = PublishingPolicy.GetPublishingPolicyResult(record);

            result.HubRecord.Should().BeTrue();
            result.HubResources.Count.Should().Be(1);
            result.HubResources.Should().BeEquivalentTo(new List<Resource> { resource });
            result.GovRecord.Should().BeFalse();
            result.GovResources.Should().BeNullOrEmpty();
            result.Message.Should().Be("This record is not fully Open Data as it has a restrictive licence.");
        }

        [Test]
        public void record_with_unknown_ownership(
            [Values("dataset", "nonGeographicDataset", "service")] string resourceType,
            [Values(@"z:\a\file\resource.txt", "http://a.url.resource")] string path)
        {
            var resource = new Resource { Name = "A named resource", Path = path };
            var recordId = Helpers.AddCollection(Guid.NewGuid().ToString());
            var record = new Record
            {
                Id = recordId,
                Path = @"X:\some\path",
                Gemini = Library.Example()
                    .With(r => r.Keywords.Add(new MetadataKeyword { Value = "Unknown Ownership" }))
                    .With(r => r.ResourceType = resourceType),
                Publication = new PublicationInfo
                {
                    Data = new DataPublicationInfo
                    {
                        Resources = new List<Resource> { resource }
                    },
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true
                    }
                }
            };
            var result = PublishingPolicy.GetPublishingPolicyResult(record);

            result.HubRecord.Should().BeTrue();
            result.HubResources.Count.Should().Be(1);
            result.HubResources.Should().BeEquivalentTo(new List<Resource> { resource });
            result.GovRecord.Should().BeFalse();
            result.GovResources.Should().BeNullOrEmpty();
            result.Message.Should().Be("This record is not fully Open Data as it has unknown ownership.");
        }

        [Test]
        public void darwin_plus_publication_still_goes_to_the_datahub()
        {
            var resource = new Resource { Name = "Data not yet available page", Path = "http://a.temporary.url" };
            var record = new Record
            {
                Id = Helpers.AddCollection(Guid.NewGuid().ToString()),
                Path = @"X:\some\path",
                Gemini = Library.Example()
                    .With(r => r.ResourceType = "publication")
                    .With(r => r.Keywords.Add(new MetadataKeyword { Value = "Darwin Plus" })),
                Publication = new PublicationInfo
                {
                    Data = new DataPublicationInfo
                    {
                        Resources = new List<Resource> { resource }
                    },
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true
                    }
                }
            };
            var result = PublishingPolicy.GetPublishingPolicyResult(record);

            result.HubRecord.Should().BeTrue();
            result.HubResources.Should().BeEquivalentTo(new List<Resource> { resource });
            result.GovRecord.Should().BeFalse();
            result.GovResources.Should().BeNullOrEmpty();
            result.Message.Should().Be("This is a JNCC publication.");
        }

        [Test]
        public void darwin_plus_record_should_not_go_to_datahub([Values("dataset", "nonGeographicDataset", "service")] string resourceType)
        {
            var resource = new Resource { Name = "Data not yet available page", Path = "http://a.temporary.url" };
            var record = new Record
            {
                Id = Helpers.AddCollection(Guid.NewGuid().ToString()),
                Path = @"X:\some\path",
                Gemini = Library.Example()
                    .With(r => r.ResourceType = resourceType)
                    .With(r => r.Keywords.Add(new MetadataKeyword { Value = "Darwin Plus" })),
                Publication = new PublicationInfo
                {
                    Data = new DataPublicationInfo
                    {
                        Resources = new List<Resource> { resource }
                    },
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true
                    }
                }
            };
            var result = PublishingPolicy.GetPublishingPolicyResult(record);

            result.HubRecord.Should().BeFalse();
            result.HubResources.Should().BeNullOrEmpty();
            result.GovRecord.Should().BeTrue();
            result.GovResources.Count.Should().Be(1);
            result.GovResources[0].Should().Be(resource.Path);
            result.Message.Should().Be("This is a Darwin Plus record.");
        }

        [Test]
        public void darwin_plus_record_with_doi_should_not_be_republished([Values("dataset", "nonGeographicDataset", "service")] string resourceType)
        {
            var resource = new Resource { Name = "Data not yet available page", Path = "http://a.temporary.url" };
            var record = new Record
            {
                Id = Helpers.AddCollection(Guid.NewGuid().ToString()),
                Path = @"X:\some\path",
                Gemini = Library.Example()
                    .With(r => r.ResourceType = resourceType)
                    .With(r => r.Keywords.Add(new MetadataKeyword { Value = "Darwin Plus" }))
                    .With(r => r.MetadataDate = DateTime.Today),
                Publication = new PublicationInfo
                {
                    Data = new DataPublicationInfo
                    {
                        Resources = new List<Resource> { resource }
                    },
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true,
                        LastSuccess = new PublicationAttempt { DateUtc = DateTime.Today }
                    }
                },
                Citation = "a citation reference",
                DigitalObjectIdentifier = "10.25603/840424.1.0.0"
            };
            var result = PublishingPolicy.GetPublishingPolicyResult(record);

            result.HubRecord.Should().BeFalse();
            result.HubResources.Should().BeNullOrEmpty();
            result.GovRecord.Should().BeFalse();
            result.GovResources.Should().BeNullOrEmpty();
            result.Message.Should().Be("This record has a DOI and cannot be republished.");
        }

        [Test]
        public void a_jncc_website_publication_with_one_resource(
            [Values(@"z:\a\file\resource.txt", "http://a.url.resource")] string path)
        {
            var resource = new Resource { Name = "Website publication resource", Path = path };
            var record = new Record
            {
                Id = Helpers.AddCollection(Guid.NewGuid().ToString()),
                Path = @"X:\some\path",
                Gemini = Library.Example()
                    .With(r => r.ResourceType = "publication"),
                Publication = new PublicationInfo
                {
                    Data = new DataPublicationInfo
                    {
                        Resources = new List<Resource> { resource }
                    },
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true
                    }
                }
            };
            var result = PublishingPolicy.GetPublishingPolicyResult(record);

            result.HubRecord.Should().BeTrue();
            result.HubResources.Should().BeEquivalentTo(new List<Resource> { resource });
            result.GovRecord.Should().BeFalse();
            result.GovResources.Should().BeNullOrEmpty();
            result.Message.Should().Be("This is a JNCC publication.");
        }

        [Test]
        public void a_jncc_website_publication_with_multiple_resources()
        {
            var resources = new List<Resource> {
                new Resource { Name = "PDF 1", Path = @"z:\a\pdf\resource.pdf" },
                new Resource { Name = "PDF 2", Path = @"z:\a\second\pdf\resource.pdf" },
                new Resource { Name = "PDF 3", Path = @"z:\a\third\pdf\resource.pdf" }
            };
            var record = new Record
            {
                Id = Helpers.AddCollection(Guid.NewGuid().ToString()),
                Path = @"X:\some\path",
                Gemini = Library.Example()
                    .With(r => r.ResourceType = "publication"),
                Publication = new PublicationInfo
                {
                    Data = new DataPublicationInfo
                    {
                        Resources = resources
                    },
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true
                    }
                }
            };
            var result = PublishingPolicy.GetPublishingPolicyResult(record);

            result.HubRecord.Should().BeTrue();
            result.HubResources.Should().BeEquivalentTo(resources);
            result.GovRecord.Should().BeFalse();
            result.GovResources.Should().BeNullOrEmpty();
            result.Message.Should().Be("This is a JNCC publication.");
        }

        [Test]
        public void a_jncc_website_publication_with_no_resources()
        {
            var record = new Record
            {
                Id = Helpers.AddCollection(Guid.NewGuid().ToString()),
                Path = @"X:\some\path",
                Gemini = Library.Example()
                    .With(r => r.ResourceType = "publication"),
                Publication = new PublicationInfo
                {
                    Data = new DataPublicationInfo
                    {
                        Resources = null
                    },
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true
                    }
                }
            };
            var result = PublishingPolicy.GetPublishingPolicyResult(record);

            result.HubRecord.Should().BeTrue();
            result.HubResources.Should().BeNullOrEmpty();
            result.GovRecord.Should().BeFalse();
            result.GovResources.Should().BeNullOrEmpty();
            result.Message.Should().Be("This is a JNCC publication.");
        }
    }
}
