using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Catalogue.Data.Import;
using Catalogue.Data.Import.Mappings;
using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Gemini.DataFormats;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
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
                var vocabService = new VocabularyService(db);
                var s = new Seeder(db, new RecordService(db, new RecordValidator(vocabService)));
                s.AddVocabularies();
                s.AddMeshRecords();
                s.AddSimpleExampleRecord();
                s.AddReadOnlyRecord();
                s.AddSecureRecords();
                s.AddNonTopCopyRecord();
                s.AddVariousDataFormatRecords();
                s.AddBboxes();
                db.SaveChanges();
            }
        }

        Record MakeExampleSeedRecord()
        {
            return new Record
            {
                Gemini = Library.Blank().With(m =>
                {
                    m.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-broad-category", Value = "Example Records" });
                    m.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/example", Value = "example" });
                }),
            };
        }

        void AddMeshRecords()
        {
            // load the seed data file from the embedded resource
            string resource = "Catalogue.Data.Seed.mesh.csv";
            var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);

            using (var reader = new StreamReader(s))
            {
                var importer = Importer.CreateImporter<MeshMapping>(db);
                importer.ImportKeywords = true;
                importer.SkipBadRecords = true; // todo remove when data export is finished
                importer.Import(reader);
            }
        }

        void AddSimpleExampleRecord()
        {
            var record = MakeExampleSeedRecord().With(r =>
            {
                r.Id = new Guid("679434f5-baab-47b9-98e4-81c8e3a1a6f9");
                r.Path = @"X:\path\to\record\data";
                r.Gemini = r.Gemini.With(m =>
                    {
                        m.Title = "A simple example record";
                        m.Abstract = "This is a simple example record.";
                    });
            });

            recordService.Insert(record);
        }

        void AddReadOnlyRecord()
        {
            var record = MakeExampleSeedRecord().With(r =>
                {
                    r.Id = new Guid("b65d2914-cbac-4230-a7f3-08d13eea1e92");
                    r.Path = @"X:\path\to\read\only\record\data";
                    r.ReadOnly = true;
                    r.Gemini = r.Gemini.With(m =>
                        {
                            m.Title = "An example read-only record";
                            m.Abstract = "This is an example read-only record.";
                        });
                });

            recordService.Insert(record);
        }

        void AddSecureRecords()
        {
            var record = this.MakeExampleSeedRecord().With(r =>
                {
                    r.Id = new Guid("89136d54-d383-4d4d-a385-ac9687596b01");
                    r.Path = @"X:\path\to\restricted\record\data";
                    r.Security = Security.Restricted;
                    r.Gemini = r.Gemini.With(m =>
                        {
                            m.Title = "An example restricted record";
                            m.Abstract = "This is an example restricted record.";
                        });
                });

            recordService.Insert(record);
        }

        void AddNonTopCopyRecord()
        {
            var record = MakeExampleSeedRecord().With(r =>
                {
                    r.Id = new Guid("94f2c217-2e45-42be-8b48-c5075401e508");
                    r.Path = @"X:\path\to\non\top\copy\record\data";
                    r.TopCopy = false;
                    r.Gemini = r.Gemini.With(m =>
                        {
                            m.Title = "An example record that is not top-copy";
                            m.Abstract = "This is an example record that is not top-copy.";
                        });
                });

            recordService.Insert(record);
        }

        void AddVariousDataFormatRecords()
        {
            // add one record per data format group

            foreach (var g in DataFormats.Known)
            {
                string n = g.Name.ToLower();

                var record = MakeExampleSeedRecord().With(r =>
                    {
                        r.Path = @"X:\path\to\" + n + @"\record\data";
                        r.TopCopy = true;
                        r.Gemini = r.Gemini.With(m =>
                            {
                                m.Title = "An example " + n + " record";
                                m.Abstract = "This is an example record for some " + n + " data";
                                m.DataFormat = (from f in g.Formats select f.Name).FirstOrDefault();
                                m.DatasetReferenceDate = "2012-01-01";
                            });
                    });

                recordService.Insert(record);
            }
        }

        void AddBboxes()
        {
            var smallBox = MakeExampleSeedRecord().With(r =>
                {
                    r.Id = new Guid("764dcdea-1231-4494-bc18-6931cc8adcee");
                    r.Path = @"Z:\path\to\small\box";
                    r.Gemini = r.Gemini.With(m =>
                        {
                            m.Title = "Small Box";
                            m.DataFormat = "csv";
                            m.BoundingBox = new BoundingBox { North = 30, South = 20, East = 60, West = 50 };
                        });
                });

           recordService.Insert(smallBox);
        }

        void AddVocabularies()
        {
            var jnccCategories = new Vocabulary
                {
                    Id = "http://vocab.jncc.gov.uk/jncc-broad-category",
                    Name = "JNCC Broad Categories",
                    Description = "The broad dataset categories used within JNCC.",
                    PublicationDate = "2013",
                    Publishable = true,
                    Keywords = new List<VocabularyKeyword>
                        {
                            new VocabularyKeyword { Value = "Seabed Habitat Maps" },
                            new VocabularyKeyword { Value = "Marine Human Activities" },
                        }
                };
            db.Store(jnccCategories);

            var referenceManagerCode = new Vocabulary
                {
                    Id = "http://vocab.jncc.gov.uk/reference-manager-code",
                    Name = "JNCC Reference Manager Code",
                    Description = "A field for the Reference Manager code used within JNCC.",
                    PublicationDate = "2013",
                    Publishable = false,
                    Controlled = false,
                    Keywords = new List<VocabularyKeyword>()
                };
            db.Store(referenceManagerCode);

            var meshOriginalSeabedClassification = new Vocabulary
                {
                    Id = "http://vocab.jncc.gov.uk/original-seabed-classification-system",
                    Name = "Original Seabed Classification",
                    Description = "Used by MESH",
                    PublicationDate = "2013",
                    Publishable = false,
                    Controlled = true,
                    Keywords = new List<VocabularyKeyword>()
                };
            db.Store(meshOriginalSeabedClassification);

            var meshSeabedMapStatus = new Vocabulary
                {
                    Id = "http://vocab.jncc.gov.uk/seabed-map-status",
                    Name = "Seabed Map Status",
                    Description = "Used by MESH",
                    PublicationDate = "2013",
                    Publishable = false,
                    Controlled = true,
                    Keywords = new List<VocabularyKeyword>()
                };
            db.Store(meshSeabedMapStatus);

            var meshSeabedSurveyPurpose = new Vocabulary
            {
                Id = "http://vocab.jncc.gov.uk/seabed-survey-purpose",
                Name = "Seabed Survey Purpose",
                Description = "Used by MESH",
                PublicationDate = "2013",
                Publishable = false,
                Controlled = true,
                Keywords = new List<VocabularyKeyword>()
            };
            db.Store(meshSeabedSurveyPurpose);

            var meshSeabedSurveyTechnique = new Vocabulary
            {
                Id = "http://vocab.jncc.gov.uk/seabed-survey-technique",
                Name = "Seabed Map Status",
                Description = "Used by MESH",
                PublicationDate = "2013",
                Publishable = false,
                Controlled = true,
                Keywords = new List<VocabularyKeyword>()
            };
            db.Store(meshSeabedSurveyTechnique);


        }

        // BigBoundingBoxWithNothingInside and SmallBox do not intersect
        // verify using http://arthur-e.github.io/Wicket/sandbox-gmaps3.html

        public static readonly string BoundingBoxContainingNothing = "POLYGON((10 10,40 10,40 40,10 40,10 10))";
        public static readonly string BoundingBoxContainingSmallBox = "POLYGON((40 10,60 10,60 30,40 30,40 10))";
    }
}
