using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using Raven.Client;
using Raven.Client.Document;

namespace Catalogue.Data
{
    public class Seeder
    {
        readonly IDocumentSession db;

        public Seeder(IDocumentSession db)
        {
            this.db = db;
        }

        public static void Seed(IDocumentStore store)
        {
            using (var db = store.OpenSession())
            {
                new Seeder(db).AddItems();
                db.SaveChanges();
            }
        }

        void AddItems()
        {
            db.Store(SmallBox);
        }

        // BigBoundingBoxWithNothingInside and SmallBox do not intersect
        // verify using http://arthur-e.github.io/Wicket/sandbox-gmaps3.html

        public static readonly string BoundingBoxContainingNothing = "POLYGON((10 10,40 10,40 40,10 40,10 10))";
        public static readonly string BoundingBoxContainingSmallBox = "POLYGON((40 10,60 10,60 30,40 30,40 10))";

        public static readonly Item SmallBox = new Item
        {
            Id = new Guid("764dcdea-1231-4494-bc18-6931cc8adcee"),
            Metadata = new Metadata
                {
                    Title = "Small Box",
                    DataFormat = new DataFormat {Name = "csv" },
                },
            Wkt = "POLYGON((50 20,60 20,60 30,50 30,50 20))",
        };
    }
}
