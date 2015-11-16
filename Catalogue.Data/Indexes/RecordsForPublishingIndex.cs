using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using Raven.Client.Indexes;

namespace Catalogue.Data.Indexes
{
    public class RecordsForPublishingIndex : AbstractIndexCreationTask<Record, RecordsForPublishingIndex.Result>
    {
        public class Result
        {
            DateTime Staleness { get; set; }
        }

        /// <summary>
        /// Calculates a value which is the length of time between the last publications and when the record was last edited.
        /// If this is a positive number, the record is "stale" with respect to publication, so needs publishing.
        /// </summary>
        public RecordsForPublishingIndex()
        {
            Map = records => from r in records
                             select new { Staleness = r.Gemini.MetadataDate - r.Publication.LastSuccess };
        }
    }
}
