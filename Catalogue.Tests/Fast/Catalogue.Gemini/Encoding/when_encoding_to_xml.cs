using System;
using System.Collections.Generic;
using System.Linq;
using Catalogue.Gemini.Encoding;
using Catalogue.Gemini.Model;
using NUnit.Framework;

namespace Catalogue.Tests.Fast.Catalogue.Gemini.Encoding
{
    class when_encoding_to_xml
    {
        [Test]
        public void should_work()
        {
            var metadata = new Metadata
                {
                    Title = "Demo Dataset",
                    Abstract = "This is just a demo dataset.",
                    TopicCategory = "geoscientificInformation",
                    Keywords = new List<string> { "sounding" },
                    TemporalExtent = new TemporalExtent { Begin = "2001-01-13", End = "2010-01-25" },
                    DatasetReferenceDate = "2012-03-17",
                    Lineage = "This dataset was imagined by a developer.",
                    DataFormat = new DataFormat { Name = "GML", Version = "3.2.1" },
                    ResponsibleOrganisation = new ResponsibleParty
                        {
                            Name = "Joint Nature Conservation Committee (JNCC)",
                            Email = "data@jncc.gov.uk",
                            Role = "distributor",
                        },
                    LimitationsOnPublicAccess = "no limitations",
                    UseConstraints = "no conditions apply",
                    MetadataDate = "2013/07/16",
                    MetadataLanguage = "eng",
                    MetadataPointOfContact = new ResponsibleParty
                        {
                            Name = "Joint Nature Conservation Committee (JNCC)",
                            Email = "some.user@jncc.gov.uk",
                            Role = "pointOfContact",
                        },
                    UniqueResourceIdentifier = "http://data.jncc.gov.uk/5eb63655-d7fe-46af-88bc-71f7db243ad3",
                    ResourceType = "dataset",
                    BoundingBox = new BoundingBox
                        {
                            North = 60.77m,
                            South = 49.79m,
                            East = 2.96m,
                            West = -8.14m,
                        }

                };

            var xml = new XmlEncoder().Encode(metadata, new Guid("5eb63655-d7fe-46af-88bc-71f7db243ad3"));
        }
    }
}
