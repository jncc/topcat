﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Catalogue.Data.Test;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.Spatial;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Indexes.Spatial;
using Raven.Client.Documents.Session;

namespace Catalogue.Tests.Explicit
{
    internal class feasibility_tests
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
                where p.North < 61 && p.South > 50 && p.East < 2 && p.West > -12
                // ignore bad data
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


            IDocumentStore store = new DocumentStore { Urls = new[] { RavenUrl } }.Initialize();

            using (IDocumentSession db = store.OpenSession())
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

            IndexCreation.CreateIndexes(typeof (Record).Assembly, store);
        }

        // raven4
        [Ignore]
        [Explicit, Test]
        public void query_gemini_records()
        {
            IDocumentStore store = new DocumentStore { Urls = new[] { RavenUrl } }.Initialize();
            //WaitForIndexing(store);

            string peakDistrictBbox = BoundingBoxUtility.ToWkt(new BoundingBox
            {
                North = 53.6m,
                South = 53.0m,
                East = -1.52m,
                West = -2.14m
            });

            Stopwatch watch = Stopwatch.StartNew();

            using (IDocumentSession db = store.OpenSession())
            {
                List<Record> results = db.Query<Record, RecordSpatialIndex>()
                    //.Customize(x => x.RelatesToShape(FieldNames.Spatial, peakDistrictBbox, SpatialRelation.Intersects))
                    .Where(i => i.Gemini.Title.StartsWith("GA"))
                    //.Take(10)
                    .ToList();

                //results.Count().Should().Be(10);
                Console.WriteLine(results.Count);
            }

            Console.WriteLine(watch.ElapsedMilliseconds);
        }

        [Explicit, Test]
        public void fizz_buzz()
        {
            foreach (int n in Enumerable.Range(1, 100))
            {
                bool isMultipleOf3 = n%3 == 0;
                bool isMultipleOf5 = n%5 == 0;

                if (isMultipleOf3 && isMultipleOf5)
                    Console.Write("FizzBuzz");
                else if (isMultipleOf5)
                    Console.Write("Buzz");
                else if (isMultipleOf3)
                    Console.Write("Fizz");
                else
                    Console.Write(n);
            }
        }
    }
}