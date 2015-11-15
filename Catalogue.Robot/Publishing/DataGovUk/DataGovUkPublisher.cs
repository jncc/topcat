using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using NUnit.Framework;
using Raven.Client;

namespace Catalogue.Robot.Publishing.DataGovUk
{
    public class DataGovUkPublisher : Publisher
    {
        IDocumentSession db;

        public DataGovUkPublisher(IDocumentSession db)
        {
            this.db = db;
        }

        public override void Publish()
        {
            // get the publishable records that have been updated since the last successful publication
            // copy the dataset from the Path to the target folder
            // encode the metadata and setting the ResourceLocator appropriately
            // add the manifest

            var q = db.Query<Record>().Where(r => r.Gemini.MetadataDate > r.Publication.LastSuccess) // todo check timezones
                .Take(1000) // todo stream
                .ToList();

            Console.WriteLine(q.Count);

        }
    }


}
