using System;
using System.Collections.Generic;

namespace Catalogue.Data.Model
{
    public class Item
    {
        public Guid Id { get; set; }

        public string   Xml      { get; set; }
        public Metadata Metadata { get; set; }
        public bool     Public   { get; set; }

        public string   Location { get; set; }
        public bool     TopCopy  { get; set; }
    }


    /// <summary>
    /// A simple implementation of the UK Gemini 2.2 metadata standard.
    /// </summary>
    public class Metadata
    {
        public string Title { get; set; }
        public string Abstract { get; set; }
        public string TopicCategory { get; set; }
        public List<string> Keywords { get; set; }
        public string TemporalExtent { get; set; }
        public string DatasetReferenceDate { get; set; }
        public string Lineage { get; set; }
        public string DataFormat { get; set; }
        public string ResponsibleOrganisation { get; set; }
        public string LimitationsOnPublicAccess { get; set; }
        public string UseConstraints { get; set; }
        public string MetadataDate { get; set; }
        public string MetadataLanguage { get; set; }
        public string MetadataPointOfContact { get; set; }
        public string UniqueResourceIdentifier { get; set; }
        public string ResourceType { get; set; }
        public BoundingBox BoundingBox { get; set; }
    }

    public class BoundingBox
    {
        public string North { get; set; }
        public string South { get; set; }
        public string East  { get; set; }
        public string West  { get; set; }
    }
}
