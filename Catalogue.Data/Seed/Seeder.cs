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
using Catalogue.Gemini.Helpers;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using Catalogue.Utilities.Collections;
using Catalogue.Utilities.Time;
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
                s.AddHumanActivitiesRecord();
                s.AddOverseasTerritoriesRecord();
                s.AddSimpleGeminiExampleRecord();
                s.AddRecordsWithPublishingInfo();
                s.AddRecordWithLotsOfVocablessTags();
                s.AddReadOnlyRecord();
                s.AddNonGeographicDataset();
                s.AddSecureRecords();
                s.AddNonTopCopyRecord();
                s.AddVariousDataFormatRecords();
                s.AddRecordWithUnusualCharactersInKeywords();
                s.AddTwoRecordsWithTheSameBoundingBox();
                s.AddBboxes();
                s.AddLastModifiedDateRecords();

                db.SaveChanges();
            }
        }

        Record MakeExampleSeedRecord()
        {
            return new Record
            {
                Gemini = Library.Blank().With(m =>
                    {
                        m.ResourceType = "dataset";
                        m.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-domain", Value = "Terrestrial" });
                        m.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-category", Value = "Example Collection" });
                        m.Keywords.Add(new MetadataKeyword { Vocab = "", Value = "example" });
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
                var importer = Importer.CreateImporter(db, new MeshMapping());
                importer.Import(reader);

//                var probs = from r in importer.Results
//                    where !r.Success
//                    select new
//                    {
//                        r.Record.SourceIdentifier,
//                        errors = r.Validation.Errors.ToConcatenatedString(e => e.Message, ",")
//                    };
//
//                string log = ObjectDumper.String(probs);
            }
        }

        void AddHumanActivitiesRecord()
        {
            var record = new Record().With(r =>
            {
                r.Id = new Guid("d8b438dc-4cd3-4d4f-9fa7-1160ea2336fd");
                r.Path = @"X:\path\to\human\activities\data";
                r.Gemini = new Metadata().With(m =>
                {
                    m.Title = "A simple Human Activity example record";
                    m.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-domain", Value = "Marine" });
                    m.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-category", Value = "Human Activities" });
                    m.ResourceType = "dataset";
                });
            });

            recordService.Insert(record);
        }

        void AddOverseasTerritoriesRecord()
        {
            var record = new Record().With(r =>
            {
                r.Id = new Guid("d836cd57-7c94-43b6-931b-3d63a58e3541");
                r.Path = @"X:\path\to\overseas\territories\data";
                r.Gemini = new Metadata().With(m =>
                {
                    m.Title = "A simple Overseas Territories example record";
                    m.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-domain", Value = "Marine" });
                    m.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-category", Value = "Overseas Territories" });
                    m.ResourceType = "dataset";
                });
            });

            recordService.Insert(record);
        }

        void AddSimpleGeminiExampleRecord()
        {
            var record = MakeExampleSeedRecord().With(r =>
            {
                r.Id = new Guid("679434f5-baab-47b9-98e4-81c8e3a1a6f9");
                r.Path = @"X:\path\to\record\data";
                r.TopCopy = true;
                r.Validation = Validation.Gemini;
                r.Gemini = r.Gemini.With(m =>
                    {
                        m.Title = "A simple Gemini-compliant example record";
                        m.Abstract = "This is a simple Gemini-compliant example record.";
                        m.TopicCategory = "environment";
                        m.TemporalExtent = new TemporalExtent { Begin = "1998", End = "2005" };
                        m.DatasetReferenceDate = "2015-04-14";
                        m.Lineage = "This dataset was imagined by a developer.";
                        m.ResourceLocator = "http://data.jncc.gov.uk/679434f5-baab-47b9-98e4-81c8e3a1a6f9";
                        m.DataFormat = "Geospatial (raster)";
                        m.ResponsibleOrganisation = new ResponsibleParty
                        {
                            Name = "Joint Nature Conservation Committee (JNCC)",
                            Email = "data@jncc.gov.uk",
                            Role = "owner",
                        };
                        m.LimitationsOnPublicAccess = "no limitations";
                        m.UseConstraints = "no conditions apply";
                        m.SpatialReferenceSystem = "http://www.opengis.net/def/crs/EPSG/0/4326";
                        m.Extent = new StringPairList().ToExtentList();
                        m.MetadataDate = Convert.ToDateTime("2015-05-07");
                        m.MetadataPointOfContact = new ResponsibleParty
                        {
                            Name = "Joint Nature Conservation Committee (JNCC)",
                            Email = "some.user@jncc.gov.uk",
                            Role = "pointOfContact",
                        };
                        m.ResourceType = "dataset";
                        m.BoundingBox = new BoundingBox
                        {
                            North = 60.77m,
                            South = 49.79m,
                            East = 2.96m,
                            West = -8.14m,
                        };
                    });
            });

            recordService.Insert(record);
        }

        void AddRecordsWithPublishingInfo()
        {
            var record = MakeExampleSeedRecord().With(r =>
            {
                r.Path = @"C:\work\test-data.csv";
                r.TopCopy = true;
                r.Validation = Validation.Gemini;
                r.Gemini = r.Gemini.With(m =>
                {
                    m.Title = "A Gemini-compliant test record for publishing";
                    m.Abstract = "This is a Gemini-compliant example record.";
                    m.Keywords = m.Keywords.With(ks => ks.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/metadata-admin", Value = "Publishable"}));
                    m.TopicCategory = "environment";
                    m.TemporalExtent = new TemporalExtent { Begin = "1998", End = "2005" };
                    m.DatasetReferenceDate = "2015-04-14";
                    m.Lineage = "This dataset was imagined by a developer.";
                    m.ResourceLocator = "";
                    m.DataFormat = "Comma Separated Values";
                    m.ResponsibleOrganisation = new ResponsibleParty
                    {
                        Name = "Joint Nature Conservation Committee (JNCC)",
                        Email = "data@jncc.gov.uk",
                        Role = "owner",
                    };
                    m.LimitationsOnPublicAccess = "no limitations";
                    m.UseConstraints = "no conditions apply";
                    m.SpatialReferenceSystem = "http://www.opengis.net/def/crs/EPSG/0/4326";
                    m.Extent = new StringPairList().ToExtentList();
                    m.MetadataDate = Convert.ToDateTime("2015-05-07");
                    m.MetadataPointOfContact = new ResponsibleParty
                    {
                        Name = "Joint Nature Conservation Committee (JNCC)",
                        Email = "some.user@jncc.gov.uk",
                        Role = "pointOfContact",
                    };
                    m.ResourceType = "dataset";
                    m.BoundingBox = new BoundingBox
                    {
                        North = 60.77m,
                        South = 49.79m,
                        East = 2.96m,
                        West = -8.14m,
                    };
                });
            });

            var neverPublishedRecord = record.With(r =>
            {
                r.Id = new Guid("bd4a2f8a-b548-4ce4-a70c-3f2fdb44005c");
                r.Gemini.Title = "A never-published record";
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        LastAttempt = null,
                        LastSuccess = null,
                    }
                };
            });


            var earlierUnsuccessfullyPublishedRecord = record.With(r => 
            {
                r.Id = new Guid("b2691fed-e421-4e48-9da9-99bd77e0b8ba");
                r.Gemini.Title = "An earlier unsuccessfully published record";
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2015, 1, 1, 11, 0, 0), Message = "Failed with a terrible error in Sector 7G"},
                        LastSuccess = null,
                    }
                };
            });

            var laterSuccessfullyPublishedRecord = record.With(r =>
            {
                r.Id = new Guid("d9c14587-90d8-4eba-b670-4cf36e45196d");
                r.Gemini.Title = "A later successfully published record";
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2016, 1, 1, 13, 0, 0) },
                        LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2016, 1, 1, 13, 0, 0) },
                    }
                };
            });

            // metadata date is set at dev/test-time seed to new DateTime(2015, 1, 1, 12, 0, 0)
            // so we need to set the publish date to earlier than this
            var updatedSinceSuccessfullyPublishedRecordAndNowPaused = record.With(r =>
            {
                r.Id = new Guid("19b8c7ab-5c33-4d55-bc1d-3762b8207a9f");
                r.Gemini.Title = "An updated since successfully published record, now paused";
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2014, 12, 31) },
                        LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2014, 12, 31) },
                        Paused = true,
                    }
                };
            });

            var recordWithAlternativeResources = record.With(r =>
            {
                r.Id = new Guid("90fe83ac-d3e4-4342-8eeb-5919b38bc670");
                r.Gemini.Title = "A record with alternative resources";
                r.Gemini.ResourceLocator = "http://example.com/this/will/get/ignored/when/published";
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        LastAttempt = null,
                        LastSuccess = null,
                        Resources = new List<Resource>
                        {
                            new Resource { Path = @"Z:\some\alternative\resource\1" },
                            new Resource { Path = @"Z:\some\alternative\resource\2" },
                        }
                    }
                };
            });

            recordService.Insert(neverPublishedRecord);
            recordService.Insert(earlierUnsuccessfullyPublishedRecord);
            recordService.Insert(laterSuccessfullyPublishedRecord);
            recordService.Insert(updatedSinceSuccessfullyPublishedRecordAndNowPaused);
            recordService.Insert(recordWithAlternativeResources);
        }

        void AddRecordWithLotsOfVocablessTags()
        {
            var record = MakeExampleSeedRecord().With(r =>
            {
                r.Id = new Guid("58fbee5e-58e6-4119-82cb-587ec383cb62");
                r.Path = @"X:\blah\blah";
                r.Gemini = r.Gemini.With(m =>
                {
                    m.Title = "An example record with lots of vocabless keywords / tags";
                    m.Abstract = "This is just another example record.";
                    m.Keywords.Add(new MetadataKeyword { Vocab = "", Value = "example tag" });
                    m.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/another-vocab", Value = "another" });
                    m.Keywords.Add(new MetadataKeyword { Vocab = "", Value = "data-services" });
                    m.Keywords.Add(new MetadataKeyword { Vocab = "", Value = "metadata" });
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

        void AddNonGeographicDataset()
        {
            var record = MakeExampleSeedRecord().With(r =>
            {
                r.Id = new Guid("397a03d6-2770-445f-9900-fdb18850b5f8");
                r.Path = @"X:\path\to\nongeographic\data";
                r.Gemini = r.Gemini.With(m =>
                {
                    m.Title = "An example non-geographic record";
                    m.Abstract = "This is an example non-geographic record.";
                    m.ResourceType = "nonGeographicDataset";
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
                    r.Security = Security.OfficialSensitive;
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

        void AddRecordWithUnusualCharactersInKeywords()
        {
            var record = MakeExampleSeedRecord().With(r =>
            {
                r.Id = new Guid("1b875458-2c17-44f8-aae0-f6ed9902e70b");
                r.Path = @"X:\path\for\record\with\unusual\characters\in\keywords";
                r.Gemini = r.Gemini.With(m =>
                {
                    m.Title = "A record with unusual characters in keywords";
                });
                // add a keyword with parentheses and hypen
                r.Gemini.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/human-activity", Value = "Extraction � Water (abstraction)" });
                r.Gemini.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/some-vocab", Value = "Two words" });
            });

            recordService.Insert(record);
        }

        void AddTwoRecordsWithTheSameBoundingBox()
        {
            var wales = new BoundingBox { North = 53.98783m, South = 50.92033m, East = -2.919512m, West = -5.682273m };

            var record1 = MakeExampleSeedRecord().With(r =>
            {
                r.Id = new Guid("50749e9a-4df5-4641-81a7-1fea04346be5");
                r.Path = @"X:\path\for\record\with\same\bounding\box\as\another\1";
                r.Gemini = r.Gemini.With(m =>
                {
                    m.Title = "A record with the same bounding box an another (1)";
                    m.BoundingBox = wales;
                });
                r.Gemini.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/some-vocab", Value = "Bounding boxes" });
            });

            var record2 = MakeExampleSeedRecord().With(r =>
            {
                r.Id = new Guid("6a969b7f-18e8-4e96-b0ea-371d8e2ba774");
                r.Path = @"X:\path\for\record\with\same\bounding\box\as\another\2";
                r.Gemini = r.Gemini.With(m =>
                {
                    m.Title = "A record with the same bounding box an another (2)";
                    m.BoundingBox = wales;
                });
                r.Gemini.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/some-vocab", Value = "Bounding boxes" });
            });

            recordService.Insert(record1);
            recordService.Insert(record2);
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
            var jnccDomain = new Vocabulary
            {
                Id = "http://vocab.jncc.gov.uk/jncc-domain",
                Name = "JNCC Domain",
                Description = "Groups metadata records into broad areas.",
                PublicationDate = "2015",
                Publishable = true,
                Controlled = true,
                Keywords = new List<VocabularyKeyword>
                        {
                            new VocabularyKeyword { Value = "Marine" },
                            new VocabularyKeyword { Value = "Freshwater" },
                            new VocabularyKeyword { Value = "Terrestrial" },
                            new VocabularyKeyword { Value = "Atmosphere" },
                        }
            };
            db.Store(jnccDomain);

            var jnccCategory = new Vocabulary
                {
                    Id = "http://vocab.jncc.gov.uk/jncc-category",
                    Name = "JNCC Category",
                    Description = "Groups metadata records into collections.",
                    PublicationDate = "2015",
                    Publishable = true,
                    Controlled = true,
                    Keywords = new List<VocabularyKeyword>
                        {
                            new VocabularyKeyword { Value = "Seabed Habitat Maps", Description = "Geospatial datasets from the Mapping European Seabed Habitats (MESH) project."},
                            new VocabularyKeyword { Value = "Human Activities", Description = "Geospatial datasets of activities undertaken by humans in the UK marine environment."},
                            new VocabularyKeyword { Value = "JNCC Publications", Description = "Official publications produced by Joint Nature Conservation Committee (JNCC)."},
                            new VocabularyKeyword { Value = "Natural Capital Library", Description = "Reports, briefings and publications related to the Natural Capital concept."},
                            new VocabularyKeyword { Value = "Example Collection", Description = "A collection of example records for development and testing."},
                        }
                };
            db.Store(jnccCategory);

            var metadataAdmin = new Vocabulary
                {
                    Id = "http://vocab.jncc.gov.uk/metadata-admin",
                    Name = "Metadata Admin",
                    Description = "Tags for managing Topcat records.",
                    PublicationDate = "2015",
                    Publishable = false,
                    Controlled = true,
                    Keywords = new List<VocabularyKeyword>
                        {
                            new VocabularyKeyword { Value = "Delete" },
                            new VocabularyKeyword { Value = "Improve" },
                            new VocabularyKeyword { Value = "Suspect" },
                        }
                };
            db.Store(metadataAdmin);

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
                Name = "Seabed Survey Technique",
                Description = "Used by MESH",
                PublicationDate = "2013",
                Publishable = false,
                Controlled = true,
                Keywords = new List<VocabularyKeyword>()
            };
            db.Store(meshSeabedSurveyTechnique);


        }

        void AddLastModifiedDateRecords()
        {
            var timeGetter = Clock.CurrentUtcDateTimeGetter;

            var tuples = new[]
            {
                Tuple.Create("8f4562ea-9d8a-45a0-afd3-bc5072d342a0", "DateTest_1",
                    new DateTime(2016, 01, 03, 09, 00, 00), "Cathy"),
                Tuple.Create("3ad98517-110b-40d7-aa0d-f0e3b1273007", "DateTest_2",
                    new DateTime(2017, 06, 05, 09, 00, 00), "Pete"),
                Tuple.Create("8c88dd97-3317-43e4-b59e-239e0604a094", "DateTest_3",
                    new DateTime(2017, 07, 01, 15, 00, 00), "Cathy"),
                Tuple.Create("f5a48ac7-13f6-40ba-85a2-f4534d9806a5", "DateTest_4",
                    new DateTime(2017, 07, 01, 14, 00, 0), "Pete"),
                Tuple.Create("80de0c30-325a-4392-ab4e-64b0654ca6ec", "DateTest_5",
                    new DateTime(2017, 07, 11, 10, 00, 00), "Pete"),
                Tuple.Create("afb567fe-8ca4-4274-9c4f-d1125983226c", "DateTest_6",
                    new DateTime(2018, 07, 12, 14, 00, 00), "Pete"),
                Tuple.Create("1458dfd1-e356-4287-9190-65e5f9ffd1df", "DateTest_7",
                    new DateTime(2017, 07, 12, 15, 00, 00), "Cathy"),
            };

            foreach (Tuple<String, String, DateTime, String> tuple in tuples)
            {
                Clock.CurrentUtcDateTimeGetter = () => tuple.Item3;

                var record = MakeExampleSeedRecord().With(r =>
                {
                    r.Id = new Guid(tuple.Item1);
                    r.Path = @"X:\path\to\last\modified\date\data\"+ tuple.Item2;
                    r.Gemini = r.Gemini.With(m =>
                    {
                        m.Title = tuple.Item2;
                        m.Abstract = "Record with different last modified date";
                        m.MetadataPointOfContact.Name = tuple.Item4;
                    });
                });

                recordService.Insert(record);
            }

            // clean up!
            Clock.CurrentUtcDateTimeGetter = timeGetter;
        }

        // BigBoundingBoxWithNothingInside and SmallBox do not intersect
        // verify using http://arthur-e.github.io/Wicket/sandbox-gmaps3.html

        public static readonly string BoundingBoxContainingNothing = "POLYGON((10 10,40 10,40 40,10 40,10 10))";
        public static readonly string BoundingBoxContainingSmallBox = "POLYGON((40 10,60 10,60 30,40 30,40 10))";
    }
}
