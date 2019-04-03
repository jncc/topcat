using System;
using Catalogue.Gemini.Model;

namespace Catalogue.Data.Model
{
    public class Record
    {
        public string Id { get; set; }

        /// <summary>
        /// The UK Gemini metadata record. 
        /// </summary>
        public Metadata Gemini  { get; set; }

        public string     Path            { get; set; }
        public Image      Image           { get; set; }
        public bool       TopCopy         { get; set; }
        public Status     Status          { get; set; }
        public Validation Validation      { get; set; }
        public Security   Security        { get; set; }
        public DateTime?  Review          { get; set; }
        public string     Notes           { get; set; }
        public UserInfo   Manager         { get; set; }

        /// <summary>
        /// DataCite digital object identifier (DOI) registered to the dataset, if any.
        /// </summary>
        public string DigitalObjectIdentifier { get; set; }
        public string Citation { get; set; } //required for DOI records

        /// <summary>
        /// An optional identifier for records imported from another source.
        /// </summary>
        public string SourceIdentifier { get; set; }
        public bool   ReadOnly         { get; set; } // for imported records

        //todo: move publishable resources and datahub url here

        public PublicationInfo Publication { get; set; }

        /// <summary>
        /// A 'well known text' representation of the bounding box in the Gemini record
        /// used for spatial indexing.
        /// </summary>
        public string Wkt { get; set; }

        /// <summary>
        /// Used by the infrastructure when representing a particular revision of the record.
        /// </summary>
        public int Revision { get; internal set; }

        public Footer Footer { get; set; }
    }
}
