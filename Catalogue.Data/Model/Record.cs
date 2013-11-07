using System;
using Catalogue.Gemini.Model;

namespace Catalogue.Data.Model
{
    public class Record
    {
        public Guid Id { get; set; }

        /// <summary>
        /// The UK Gemini metadata record. 
        /// </summary>
        public Metadata Gemini  { get; set; }

        /// <summary>
        /// A 'well known text' representation of the bounding box in the Gemini record
        /// used for spatial indexing.
        /// </summary>
        public string   Wkt      { get; set; }

        public Status   Status   { get; set; }
        public bool     TopCopy  { get; set; }
        public string   Notes    { get; set; }

        /// <summary>
        /// An optional identifier for records imported from another source.
        /// </summary>
        public string SourceIdentifier { get; set; }
        public bool   ReadOnly         { get; set; } // for imported records
    }
}
