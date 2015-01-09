﻿using System;
using System.Collections.Generic;

namespace Catalogue.Gemini.Model
{
    /// <summary>
    /// A simple implementation of the UK Gemini 2.2 metadata standard.
    /// </summary>
    public class Metadata
    {
        public string Title { get; set; }
        public string Abstract { get; set; }
        public string TopicCategory { get; set; }
        public List<MetadataKeyword> Keywords { get; set; }
        public TemporalExtent TemporalExtent { get; set; }
        public DateTime DatasetReferenceDate { get; set; } // / this should be changed to a collection for creation/publication/revision
        public string Lineage { get; set; }
//      public decimal SpatialResolution { get; set; } // todo we'll probably need this https://wiki.ceh.ac.uk/display/cehigh/Spatial+resolution
        public string ResourceLocator { get; set; }
        public string AdditionalInformationSource { get; set; }
        public string DataFormat { get; set; } // mesh uses MEDIN data format categories from http://vocab.ndg.nerc.ac.uk/client/vocabServer.jsp
        public ResponsibleParty ResponsibleOrganisation { get; set; }
        public string LimitationsOnPublicAccess { get; set; }
        public string UseConstraints { get; set; }
        public string SpatialReferenceSystem { get; set; }
        public List<Extent> Extent { get; set; } // support multiple locations; use same UI as keywords
        public DateTime MetadataDate { get; set; }
        
        public ResponsibleParty MetadataPointOfContact { get; set; }
        public string ResourceType { get; set; }  // dataset | series | service
        public BoundingBox BoundingBox { get; set; }

        public Metadata()
        {
            Keywords = new List<MetadataKeyword>();
            TemporalExtent = new TemporalExtent();
            ResponsibleOrganisation = new ResponsibleParty();
            MetadataPointOfContact = new ResponsibleParty();
            BoundingBox = new BoundingBox();
        }
    }

    /// <summary>
    /// lowercase to match gemini spec
    /// </summary>
    public enum SupportedLanguage
    {
        eng = 1,
        deu =2,
        fra =3,
        fin = 4
    }

    public class MetadataKeyword
    {
        public string Value { get; set; }
        public string Vocab { get; set; }
    }

    public class Extent
    {
        public string Value { get; set; }
        public string Authority { get; set; }
    }

    public class TemporalExtent
    {
        public DateTime Begin { get; set; }
        public DateTime End { get; set; }
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
