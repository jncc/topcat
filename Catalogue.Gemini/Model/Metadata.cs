using System;
using System.Collections.Generic;

namespace Catalogue.Gemini.Model
{
    /// <summary>
    /// A simple implementation of the UK Gemini 2.2 metadata standard.
    /// </summary>
    public class Metadata
    {
        /// <summary>
        /// The "id" is not actually part of the metadata record itself, but is convenient to have available.
        /// </summary>
        public Guid FileIdentifier { get; set; }

        public string Title { get; set; }
        public string Abstract { get; set; }
        public string TopicCategory { get; set; }
        public List<Keyword> Keywords { get; set; }
        public TemporalExtent TemporalExtent { get; set; }
        public string DatasetReferenceDate { get; set; }
        public string Lineage { get; set; }
//      public decimal SpatialResolution { get; set; } // todo we'll probably need this https://wiki.ceh.ac.uk/display/cehigh/Spatial+resolution
        public string ResourceLocator { get; set; }
        public DataFormat DataFormat { get; set; }
        public ResponsibleParty ResponsibleOrganisation { get; set; }
        public string LimitationsOnPublicAccess { get; set; }
        public string UseConstraints { get; set; }
        public string SpatialReferenceSystem { get; set; }
//      public string Extent { get; set; } todo i think we need this for Known Locations
        public string MetadataDate { get; set; }
        public string MetadataLanguage { get; set; }
        public ResponsibleParty MetadataPointOfContact { get; set; }
        public string UniqueResourceIdentifier { get; set; }
        public string ResourceType { get; set; }
        public BoundingBox BoundingBox { get; set; }

        public Metadata()
        {
            Keywords = new List<Keyword>();
        }
    }

    public class Keyword
    {
        public string Value { get; set; }
        public string Vocab { get; set; }
    }

    public class DataFormat
    {
        public string Name { get; set; }
        public string Version { get; set; }
    }

    public class TemporalExtent
    {
        public string Begin { get; set; }
        public string End { get; set; }
    }

    public class ResponsibleParty
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }

    /// <summary>
    ///  Bounding box referenced to WGS 84.
    /// </summary>
    public class BoundingBox
    {
        public decimal North { get; set; }
        public decimal South { get; set; }
        public decimal East { get; set; }
        public decimal West { get; set; }
    }

    public class Citation
    {
        public string IdentifierUrl { get; set; }
        public string Name { get; set; }
    }
}
