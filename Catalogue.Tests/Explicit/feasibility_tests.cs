using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using NUnit.Framework;
using Raven.Abstractions.Extensions;

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
                     select new
                         {
                             File = Path.GetFileName(f),
                             North = xml.Descendants(gmd + "northBoundLatitude").Single().Value,
                             South = xml.Descendants(gmd + "southBoundLatitude").Single().Value,
                             East = xml.Descendants(gmd + "eastBoundLongitude").Single().Value,
                             West = xml.Descendants(gmd + "westBoundLongitude").Single().Value,
                         }).ToList();

            Console.WriteLine(q.Count());
            q.ForEach(Console.WriteLine);
        }
    }
}
