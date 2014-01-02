using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Catalogue.Data;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Catalogue.Data.Test;
using Catalogue.Gemini.Model;
using Catalogue.Tests.Utility;
using Catalogue.Utilities.Spatial;
using FluentAssertions;
using NUnit.Framework;
using Raven.Abstractions.Extensions;
using Raven.Abstractions.Indexing;
using Raven.Client.Document;
using Raven.Client.Indexes;

namespace Catalogue.Tests.Explicit
{
    class feasibility_tests
    {
        private readonly string RavenUrl = "http://localhost:8081/";

        [Explicit, Test]
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
                     where p.North < 61 && p.South > 50 && p.East < 2 && p.West > -12 // ignore bad data
                     select new
                         {
                             File = Path.GetFileName(f),
                             Wkt = BoundingBoxUtility.ToWkt(new BoundingBox
                                 {
                                     North = p.North,
                                     South = p.South,
                                     East = p.East,
                                     West = p.West,
                                 })
                         })
                         .ToList();

            Console.WriteLine(q.Count());
            q.ForEach(Console.WriteLine);


            var store = new DocumentStore { Url = RavenUrl }.Initialize();
            
            using (var db = store.OpenSession())
            {
                foreach (var x in q)
                {
                    var item = new Record
                        {
                            Gemini = new Metadata
                                {
                                    Title = x.File,
                                },
                            Wkt = x.Wkt,
                        };

                    db.Store(item);
                }

                db.SaveChanges();
                
            }

            IndexCreation.CreateIndexes(typeof(Record).Assembly, store);
        }

        [Explicit, Test]
        public void query_gemini_records()
        {
            var store = new DocumentStore { Url = RavenUrl }.Initialize();
            RavenUtility.WaitForIndexing(store);

            string peakDistrictBbox = BoundingBoxUtility.ToWkt(new BoundingBox
                {
                    North = 53.6m,
                    South = 53.0m,
                    East = -1.52m,
                    West = -2.14m
                });

            var watch = Stopwatch.StartNew();

            using (var db = store.OpenSession())
            {
                var results = db.Query<Record, Records_SpatialIndex>()
                    .Customize(x => x.RelatesToShape(FieldNames.Spatial, peakDistrictBbox, SpatialRelation.Intersects))
                    .Where(i => i.Gemini.Title.StartsWith("GA"))
                    //.Take(10)
                    .ToList();

                //results.Count().Should().Be(10);
                Console.WriteLine(results.Count);
            }

            Console.WriteLine(watch.ElapsedMilliseconds);
        }

        [Explicit, Test]
        public void generate_random_boxes()
        {
            
        }
    }
}
