using System;
using System.Collections.Generic;
using System.Linq;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.Helpers;
using Catalogue.Utilities.Collections;

namespace Catalogue.Gemini.Templates
{
    /// <summary>
    /// A library of pre-made metadata instances.
    /// </summary>
    public static class Library
    {
        /// <summary>
        /// This is the important template, used for creating new records. (todo)
        /// </summary>
        public static Metadata Blank()
        {
            return new Metadata
            {
                Title = "",
                Abstract = "",
                TopicCategory = "",
                Keywords = new StringPairList().ToKeywordList(),
                TemporalExtent = new TemporalExtent { Begin = "", End = "" },
                DatasetReferenceDate = "",
                Lineage = "",
                ResourceLocator = "",
                DataFormat = "",
                ResponsibleOrganisation = new ResponsibleParty
                {
                    Name = "Joint Nature Conservation Committee (JNCC)",
                    Email = "data@jncc.gov.uk",
                    Role = "distributor",
                },
                LimitationsOnPublicAccess = "",
                UseConstraints = "",
                SpatialReferenceSystem = "",
                Extent = new List<Extent>(),
                MetadataDate = "",
                MetadataLanguage = "",
                MetadataPointOfContact = new ResponsibleParty
                {
                    Name = "",
                    Email = "",
                    Role = "",
                },
                ResourceType = "",
                BoundingBox = new BoundingBox
                {
                    North = 0m,
                    South = 0m,
                    East = 0m,
                    West = 0m,
                }
            };
        }

        public static Metadata Example()
        {
            return new Metadata
            {
                Title = "Demo Dataset",
                Abstract = "This is just a demo dataset.",
                TopicCategory = "geoscientificInformation",
                Keywords = new StringPairList
                    {
                       /* { "", "NDGO0001" },*/
                        { "http://jncc.gov.uk", "Bermuda Institute of Ocean Sciences" },
                    }
                    .ToKeywordList(),
                TemporalExtent = new TemporalExtent { Begin = "2001-01-13", End = "2010-01-25" },
                DatasetReferenceDate = "2012-03-17",
                Lineage = "This dataset was imagined by a developer.",
                ResourceLocator = "http://data.jncc.gov.uk/5eb63655-d7fe-46af-88bc-71f7db243ad3",
                DataFormat = "XLS",
                ResponsibleOrganisation = new ResponsibleParty
                {
                    Name = "Joint Nature Conservation Committee (JNCC)",
                    Email = "data@jncc.gov.uk",
                    Role = "owner",
                },
                LimitationsOnPublicAccess = "no limitations",
                UseConstraints = "no conditions apply",
                SpatialReferenceSystem = "http://www.opengis.net/def/crs/EPSG/0/4326",
                Extent = new StringPairList { { "", "Bermuda" } }.ToExtentList(),
                MetadataDate = "2013-07-16",
                MetadataLanguage = "eng",
                MetadataPointOfContact = new ResponsibleParty
                {
                    Name = "Joint Nature Conservation Committee (JNCC)",
                    Email = "some.user@jncc.gov.uk",
                    Role = "pointOfContact",
                },
                ResourceType = "dataset",
                BoundingBox = new BoundingBox
                {
                    North = 60.77m,
                    South = 49.79m,
                    East = 2.96m,
                    West = -8.14m,
                }
            };
        }
    }
}
