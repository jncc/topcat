using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Catalogue.Data.Import;
using Catalogue.Data.Import.Mappings;
using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Gemini.Model;
using Raven.Client;

namespace Catalogue.Data.Seed
{
    public class Seeder
    {
        readonly IDocumentSession db;
        readonly IRecordService recordService;

        public Seeder(IDocumentSession db, IRecordService recordService)
        {
            this.db = db;
            this.recordService = recordService;
        }

        public static void Seed(IDocumentStore store)
        {
            using (var db = store.OpenSession())
            {
                var s = new Seeder(db, new RecordService(db, new RecordValidator()));

                s.AddVocabularies();
                s.AddMeshRecords();
                s.AddReadOnlyRecord();
                s.AddBboxes();

                db.SaveChanges();
            }
        }

        void AddMeshRecords()
        {
            // load the seed data file from the embedded resource
            string resource = "Catalogue.Data.Seed.mesh.csv";
            var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);

            using (var reader = new StreamReader(s))
            {
                var importer = new Importer<MeshMapping>(new FileSystem(), new RecordService(db, new RecordValidator()));
                importer.SkipBadRecords = true; // todo remove when data export is finished
                importer.Import(reader);
            }
        }

        void AddReadOnlyRecord()
        {
            var record = new Record
                {
                    Id = new Guid("b65d2914-cbac-4230-a7f3-08d13eea1e92"),
                    Gemini = new Metadata
                        {
                            Title = "An example read-only record",
                            Abstract = "This is an example read-only record.",
                            ResourceLocator = @"X:\some\location",
                        },
                    ReadOnly = true,
                };

            recordService.Insert(record);
        }

        void AddBboxes()
        {
            this.db.Store(SmallBox);
        }

        void AddVocabularies()
        {
            var jnccCategories = new Vocabulary
                {
                    Id = "http://vocab.jncc.gov.uk/jncc-broad-categories",
                    Name = "JNCC Broad Categories",
                    Description = "The broad dataset categories used within JNCC.",
                    PublicationDate = "2013",
                    Values = new List<string>
                        {
                            "marine-habitat-data",
                            "marine-activities-pressures-data",
                        }
                };
            this.db.Store(jnccCategories);
        }

        // BigBoundingBoxWithNothingInside and SmallBox do not intersect
        // verify using http://arthur-e.github.io/Wicket/sandbox-gmaps3.html

        public static readonly string BoundingBoxContainingNothing = "POLYGON((10 10,40 10,40 40,10 40,10 10))";
        public static readonly string BoundingBoxContainingSmallBox = "POLYGON((40 10,60 10,60 30,40 30,40 10))";

        public static readonly Record SmallBox = new Record
        {
            Id = new Guid("764dcdea-1231-4494-bc18-6931cc8adcee"),
            Gemini = new Metadata
                {
                    Title = "Small Box",
                    DataFormat = new DataFormat {Name = "csv" },
                },
            Wkt = "POLYGON((50 20,60 20,60 30,50 30,50 20))",
        };
    }
}
