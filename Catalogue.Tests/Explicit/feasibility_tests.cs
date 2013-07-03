using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Catalogue.Data.Model;
using Catalogue.Tests.Utility;
using Catalogue.Utilities.Spatial;
using NUnit.Framework;
using Raven.Abstractions.Extensions;
using Raven.Client.Document;
using Raven.Client.Indexes;

namespace Catalogue.Tests.Explicit
{
    class feasibility_tests
    {
        [Explicit]
        [Test]
        public void load_gemini_records()
        {
            string dir = @"C:\Users\PETMON\Downloads\nbn-gemini2";
            XNamespace gmd = "http://www.isotc211.org/2005/gmd";

            var q = (from f in Directory.GetFiles(dir, "*.xml")
                     where File.ReadLines(f).Any()
                     let xml = XDocument.Load(f)
                     let p = new
                         {
                             North = (decimal) xml.Descendants(gmd + "northBoundLatitude").Single(),
                             South = (decimal) xml.Descendants(gmd + "southBoundLatitude").Single(),
                             East = (decimal) xml.Descendants(gmd + "eastBoundLongitude").Single(),
                             West = (decimal) xml.Descendants(gmd + "westBoundLongitude").Single(),
                         }
                     select new
                         {
                             File = Path.GetFileName(f),
                             Wkt = BoundingBoxUtility.GetWkt(p.North, p.South, p.East, p.West)
                         }).ToList();

            Console.WriteLine(q.Count());
            q.ForEach(Console.WriteLine);


            var store = new DocumentStore { Url = "http://jncc-dev:8090/"}.Initialize();
            
            using (var db = store.OpenSession())
            {
                foreach (var x in q)
                {
                    var item = new Item
                        {
                            Metadata = new Metadata
                                {
                                    Title = x.File,
                                    BoundingBox = x.Wkt,
                                }
                        };

                    db.Store(item);
                }

                db.SaveChanges();
                
            }

            IndexCreation.CreateIndexes(typeof(Item).Assembly, store);
            RavenUtility.WaitForIndexing(store);
        }
    }
}
