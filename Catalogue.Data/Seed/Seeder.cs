using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Catalogue.Data.Import;
using Catalogue.Data.Import.Mappings;
using Catalogue.Data.Model;
using Catalogue.Data.Query;
using Catalogue.Data.Write;
using Catalogue.Gemini.DataFormats;
using Catalogue.Gemini.Helpers;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using Catalogue.Utilities.Collections;
using Catalogue.Utilities.Time;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace Catalogue.Data.Seed
{
    public class Seeder
    {
        readonly IDocumentSession db;
        readonly IRecordService recordService;
        readonly UserInfo userInfo;

        public Seeder(IDocumentSession db, IRecordService recordService)
        {
            this.db = db;
            this.recordService = recordService;
            userInfo = new UserInfo
            {
                DisplayName = "Guest",
                Email = "jncc@jncc.gov.uk"
            };
        }

        public static void Seed(IDocumentStore store)
        {
            using (var db = store.OpenSession())
            {
                var s = new Seeder(db, new RecordService(db, new RecordValidator(new VocabQueryer(db))));
                var timeGetter = Clock.CurrentUtcDateTimeGetter;
                Clock.CurrentUtcDateTimeGetter = () => new DateTime(2015, 1, 1, 12, 0, 0);

                s.AddVocabularies();
                s.AddMeshRecords();
                s.AddHumanActivitiesRecord();
                s.AddOverseasTerritoriesRecord();
                s.AddSimpleGeminiExampleRecord();
                s.AddRecordsWithPublishingInfo();
                s.AddRecordWithLotsOfVocablessTags();
                s.AddRecordsWithSameKeywordsInDifferentVocabs();
                s.AddReadOnlyRecord();
                s.AddNonGeographicDataset();
                s.AddSecureRecords();
                s.AddNonTopCopyRecord();
                s.AddVariousDataFormatRecords();
                s.AddRecordWithUnusualCharactersInKeywords();
                s.AddTwoRecordsWithTheSameBoundingBox();
                s.AddBboxes();
                s.AddDatesForTimeline();

                db.SaveChanges();

                Clock.CurrentUtcDateTimeGetter = timeGetter;
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
                Footer = new Footer
                {
                    CreatedOnUtc = Clock.NowUtc,
                    CreatedByUser = userInfo,
                    ModifiedOnUtc = Clock.NowUtc,
                    ModifiedByUser = userInfo
                }
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
            }
        }

        void AddHumanActivitiesRecord()
        {
            var record = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection("d8b438dc-4cd3-4d4f-9fa7-1160ea2336fd");
                r.Path = @"X:\path\to\human\activities\data";
                r.Gemini = new Metadata().With(m =>
                {
                    m.Title = "A simple Human Activity example record";
                    m.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-domain", Value = "Marine" });
                    m.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-category", Value = "Human Activities" });
                    m.ResourceType = "dataset";
                });
                r.Footer = new Footer
                {
                    CreatedOnUtc = Clock.NowUtc,
                    CreatedByUser = userInfo,
                    ModifiedOnUtc = Clock.NowUtc,
                    ModifiedByUser = userInfo
                };
            });

            recordService.Insert(record, userInfo);
        }

        void AddOverseasTerritoriesRecord()
        {
            var record = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection("d836cd57-7c94-43b6-931b-3d63a58e3541");
                r.Path = @"X:\path\to\overseas\territories\data";
                r.Gemini = new Metadata().With(m =>
                {
                    m.Title = "A simple Overseas Territories example record";
                    m.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-domain", Value = "Marine" });
                    m.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-category", Value = "Overseas Territories" });
                    m.ResourceType = "dataset";
                });
                r.Footer = new Footer
                {
                    CreatedOnUtc = Clock.NowUtc,
                    CreatedByUser = userInfo,
                    ModifiedOnUtc = Clock.NowUtc,
                    ModifiedByUser = userInfo
                };
            });

            recordService.Insert(record, userInfo);
        }

        void AddSimpleGeminiExampleRecord()
        {
            var record = MakeExampleSeedRecord().With(r =>
            {
                r.Id = Helpers.AddCollection("679434f5-baab-47b9-98e4-81c8e3a1a6f9");
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
                        m.ResourceType = "publication";
                        m.BoundingBox = new BoundingBox
                        {
                            North = 60.77m,
                            South = 49.79m,
                            East = 2.96m,
                            West = -8.14m,
                        };
                    });
                r.Manager = userInfo;
            });

            recordService.Insert(record, userInfo);
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

            var assessmentCompletedOnSpreadsheetRecord = record.With(r =>
            {
                r.Id = Helpers.AddCollection("471da4f2-d9e2-4a5a-b72b-3ae8cc40ae57");
                r.Gemini.Title = "A record with assessment completed on spreadsheet";
                r.Gemini.MetadataDate = new DateTime(2015, 1, 1, 09, 59, 59);
                r.Resources = new List<Resource>
                {
                    new Resource
                    {
                        Name = "File resource",
                        Path = @"C:\work\test-data.csv"
                    }
                };
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedOnUtc = DateTime.MinValue,
                        InitialAssessmentWasDoneOnSpreadsheet = true
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = DateTime.MinValue
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true,
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true,
                            LastAttempt = new PublicationAttempt
                            {
                                DateUtc = new DateTime(2015, 1, 1, 10, 0, 0)
                            },
                            LastSuccess = new PublicationAttempt
                            {
                                DateUtc = new DateTime(2015, 1, 1, 10, 0, 0)
                            }
                        }
                    }
                };
            });

            var assessedButNotCompletelyRecord = record.With(r =>
            {
                r.Id = Helpers.AddCollection("39f9442a-45e5-464f-8b20-876051560964");
                r.Gemini.Title = "A record with an incomplete risk-assessment";
                r.Gemini.MetadataDate = new DateTime(2015, 05, 17, 0, 0, 0);
                r.Resources = new List<Resource>
                {
                    new Resource
                    {
                        Name = "File resource",
                        Path = @"C:\work\test-data.csv"
                    }
                };
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        // todo add more assessment fields
                        Completed = false
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true,
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true
                        }
                    }
                };
            });

            var assessedButNotSignedOffRecord= record.With(r =>
            {
                r.Id = Helpers.AddCollection("46003050-66f3-4fb2-b2b0-f66b382c8d37");
                r.Gemini.Title = "An assessed but not signed-off record";
                r.Gemini.MetadataDate = new DateTime(2015, 1, 1, 12, 0, 0);
                r.Resources = new List<Resource>
                {
                    new Resource
                    {
                        Name = "File resource",
                        Path = @"C:\work\test-data.csv"
                    }
                };
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        // todo add more assessment fields
                        Completed = true,
                        CompletedByUser = new UserInfo
                        {
                            DisplayName = "Test User",
                            Email = "Test.user@jncc.gov.uk"
                        },
                        CompletedOnUtc = new DateTime(2015, 1, 1, 12, 0, 0)
                    },
                    SignOff = null,
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true,
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true
                        }
                    }
                };
            });

            var assessedRecordWithUrlResource = record.With(r =>
            {
                r.Id = Helpers.AddCollection("d869ae04-a3eb-477e-b8aa-66bea3e84730");
                r.Gemini.Title = "An assessed record with url resource";
                r.Gemini.MetadataDate = new DateTime(2015, 1, 1, 12, 0, 0);
                r.Resources = new List<Resource>
                {
                    new Resource
                    {
                        Name = "Web resource",
                        Path = @"http://a.web.resource"
                    }
                };
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        // todo add more assessment fields
                        Completed = true,
                        CompletedByUser = new UserInfo
                        {
                            DisplayName = "Test User",
                            Email = "Test.user@jncc.gov.uk"
                        },
                        CompletedOnUtc = new DateTime(2015, 1, 1, 12, 0, 0)
                    },
                    SignOff = null,
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true,
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true
                        }
                    }
                };
            });

            var assessedRecordWithMixedResources = record.With(r =>
            {
                r.Id = Helpers.AddCollection("2d6ab878-844d-4759-91a1-3796f92667ab");
                r.Gemini.Title = "An assessed record with file and url resources";
                r.Gemini.MetadataDate = new DateTime(2015, 1, 1, 12, 0, 0);
                r.Resources = new List<Resource>
                {
                    new Resource
                    {
                        Name = "Web resource",
                        Path = @"http://a.web.resource"
                    },
                    new Resource
                    {
                        Name = "File resource",
                        Path = @"C:\work\test-data.csv"
                    }
                };
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        // todo add more assessment fields
                        Completed = true,
                        CompletedByUser = new UserInfo
                        {
                            DisplayName = "Test User",
                            Email = "Test.user@jncc.gov.uk"
                        },
                        CompletedOnUtc = new DateTime(2015, 1, 1, 12, 0, 0)
                    },
                    SignOff = null,
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true,
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true
                        }
                    }
                };
            });

            var signedOffRecord = record.With(r =>
            {
                r.Id = Helpers.AddCollection("82ee6baf-26dc-438d-a579-dc7bcbdd1688");
                r.Gemini.Title = "A signed-off record for publication";
                r.Gemini.MetadataDate = new DateTime(2015, 1, 1, 12, 0, 0);
                r.Resources = new List<Resource>
                {
                    new Resource
                    {
                        Name = "File resource",
                        Path = @"C:\work\test-data.csv"
                    }
                };
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        // todo add more assessment fields
                        Completed = true,
                        CompletedByUser = userInfo,
                        CompletedOnUtc = new DateTime(2014, 12, 06)
                    },
                    SignOff = new SignOffInfo
                    {
                        User = new UserInfo
                        {
                            DisplayName = "Test User",
                            Email = "Test.user@jncc.gov.uk"
                        },
                        DateUtc = new DateTime(2015, 1, 1, 12, 0, 0),
                        Comment = "All OK now."
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true,
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true
                        }
                    }
                };
            });

            var signedOffUnpublishableRecord = record.With(r =>
            {
                r.Id = Helpers.AddCollection("d2885b03-8efd-472d-8a55-3e9c41a68bd7");
                r.Gemini.Title = "A DGU unpublishable record ready for publication";
                r.Gemini.MetadataDate = new DateTime(2015, 1, 1, 12, 0, 0);
                r.Resources = new List<Resource>
                {
                    new Resource
                    {
                        Name = "File resource",
                        Path = @"C:\work\test-data.csv"
                    }
                };
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedByUser = userInfo,
                        CompletedOnUtc = new DateTime(2014, 12, 06)
                    },
                    SignOff = new SignOffInfo
                    {
                        User = new UserInfo
                        {
                            DisplayName = "Test User",
                            Email = "Test.user@jncc.gov.uk"
                        },
                        DateUtc = new DateTime(2015, 1, 1, 12, 0, 0)
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = false
                        }
                    }
                };
            });

            var signedOffPublishableUnknownRecord = record.With(r =>
            {
                r.Id = Helpers.AddCollection("bcbc2a46-cf2e-4389-8294-15e01111ebda");
                r.Gemini.Title = "A DGU publishable status unknown record ready for publication";
                r.Gemini.MetadataDate = new DateTime(2015, 1, 1, 12, 0, 0);
                r.Resources = new List<Resource>
                {
                    new Resource
                    {
                        Name = "File resource",
                        Path = @"C:\work\test-data.csv"
                    }
                };
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedByUser = userInfo,
                        CompletedOnUtc = new DateTime(2014, 12, 06)
                    },
                    SignOff = new SignOffInfo
                    {
                        User = new UserInfo
                        {
                            DisplayName = "Test User",
                            Email = "Test.user@jncc.gov.uk"
                        },
                        DateUtc = new DateTime(2015, 1, 1, 12, 0, 0)
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = null
                        }
                    }
                };
            });


            var neverPublishedRecord = record.With(r =>
            {
                r.Id = Helpers.AddCollection("bd4a2f8a-b548-4ce4-a70c-3f2fdb44005c");
                r.Gemini.Title = "A never-published record";
                r.Publication = null;
            });


            var earlierUnsuccessfullyPublishedRecord = record.With(r => 
            {
                r.Id = Helpers.AddCollection("b2691fed-e421-4e48-9da9-99bd77e0b8ba");
                r.Gemini.Title = "An earlier unsuccessfully published record";
                r.Gemini.MetadataDate = new DateTime(2015, 1, 1, 12, 0, 0);
                r.Resources = new List<Resource>
                {
                    new Resource
                    {
                        Name = "File resource",
                        Path = @"C:\work\test-data.csv",
                        PublishedUrl = "https://the.hosted.location"
                    }
                };
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedByUser = userInfo,
                        CompletedOnUtc = new DateTime(2014, 01, 01)
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2014, 02, 01),
                        User = userInfo
                    },
                    Data = new DataInfo
                    {
                        LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2015, 1, 1, 11, 58, 0) },
                        LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2015, 1, 1, 11, 58, 0) }
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true,
                            Url = "https://hub.jncc.gov.uk/assets/b2691fed-e421-4e48-9da9-99bd77e0b8ba",
                            LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2015, 1, 1, 11, 59, 0) },
                            LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2015, 1, 1, 11, 59, 0) }
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true,
                            LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2015, 1, 1, 12, 0, 0), Message = "Failed with a terrible error in Sector 7G" },
                            LastSuccess = null
                        }
                    }
                };
            });

            var laterSuccessfullyPublishedRecord = record.With(r =>
            {
                r.Id = Helpers.AddCollection("d9c14587-90d8-4eba-b670-4cf36e45196d");
                r.Gemini.Title = "A later successfully published record";
                r.Gemini.MetadataDate = new DateTime(2015, 1, 1, 12, 0, 0);
                r.Resources = new List<Resource>
                {
                    new Resource
                    {
                        Name = "File resource",
                        Path = @"C:\work\test-data.csv",
                        PublishedUrl = "https://the.hosted.location"
                    }
                };
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedByUser = userInfo,
                        CompletedOnUtc = new DateTime(2014, 01, 01)
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2014, 01, 02),
                        User = userInfo
                    },
                    Data = new DataInfo
                    {
                        LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2015, 1, 1, 11, 58, 0) },
                        LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2015, 1, 1, 11, 58, 0) }
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true,
                            Url = "https://hub.jncc.gov.uk/assets/d9c14587-90d8-4eba-b670-4cf36e45196d",
                            LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2015, 1, 1, 11, 59, 0) },
                            LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2015, 1, 1, 11, 59, 0) }
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true,
                            LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2015, 1, 1, 12, 0, 0) },
                            LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2015, 1, 1, 12, 0, 0) }
                        }
                    }
                };
            });

            // metadata date is set at dev/test-time seed to new DateTime(2015, 1, 1, 12, 0, 0)
            // so we need to set the publish date to earlier than this
            var updatedSinceSuccessfullyPublishedRecord = record.With(r =>
            {
                r.Id = Helpers.AddCollection("19b8c7ab-5c33-4d55-bc1d-3762b8207a9f");
                r.Gemini.Title = "An updated since successfully published record";
                r.Gemini.MetadataDate = new DateTime(2015, 1, 1, 12, 0, 0);
                r.Resources = new List<Resource>
                {
                    new Resource
                    {
                        Name = "File resource",
                        Path = @"C:\work\test-data.csv",
                        PublishedUrl = "https://the.hosted.location"
                    }
                };
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedOnUtc = new DateTime(2014, 12, 28),
                        InitialAssessmentWasDoneOnSpreadsheet = true
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2014, 12, 29),
                        User = userInfo
                    },
                    Target = new TargetInfo
                    {
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true,
                            LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2014, 12, 31) },
                            LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2014, 12, 31) }
                        }
                    }
                };
            });

            var recordWithOpenDataResources = record.With(r =>
            {
                r.Id = Helpers.AddCollection("90fe83ac-d3e4-4342-8eeb-5919b38bc670");
                r.Gemini.Title = "A record with multiple open data resources";
                r.Gemini.MetadataDate = new DateTime(2014, 12, 31);
                r.Resources = new List<Resource>
                {
                    new Resource
                    {
                        Name = "File resource",
                        Path = @"Z:\some\resource\1.xlsx"
                    },
                    new Resource
                    {
                        Name = "File resource with published URL",
                        Path = @"Z:\some\resource\2.pdf",
                        PublishedUrl = "https://data.example.com/resources/2.pdf"
                    },
                    new Resource
                    {
                        Name = "Web resource",
                        Path = @"https://link.to.resource"
                    }
                };
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedByUser = userInfo,
                        CompletedOnUtc = new DateTime(2014, 12, 31)
                    },
                    Target = new TargetInfo
                    {
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true,
                            LastAttempt = null,
                            LastSuccess = null
                        }
                    }
                };
            });

            var unpublishableRecord = record.With(r =>
            {
                r.Id = Helpers.AddCollection("fef1d883-b6ea-4ade-93ae-8bf0abd4e29b");
                r.Gemini.Title = "A record which is unpublishable";
                r.Gemini.MetadataDate = new DateTime(2014, 12, 31);
                r.Publication = new PublicationInfo
                {
                    Target = new TargetInfo
                    {
                        Gov = new GovPublicationInfo
                        {
                            Publishable = false
                        }
                    }
                };
            });

            var timeGetter = Clock.CurrentUtcDateTimeGetter;
            Clock.CurrentUtcDateTimeGetter = () => new DateTime(2015, 1, 1, 09, 59, 59);
            recordService.Insert(assessmentCompletedOnSpreadsheetRecord, userInfo);
            Clock.CurrentUtcDateTimeGetter = timeGetter;

            recordService.Insert(neverPublishedRecord, userInfo);
            recordService.Insert(assessedButNotCompletelyRecord, userInfo);
            recordService.Insert(assessedButNotSignedOffRecord, userInfo);
            recordService.Insert(signedOffRecord, userInfo);
            recordService.Insert(earlierUnsuccessfullyPublishedRecord, userInfo);
            recordService.Insert(laterSuccessfullyPublishedRecord, userInfo);
            recordService.Insert(updatedSinceSuccessfullyPublishedRecord, userInfo);
            recordService.Insert(recordWithOpenDataResources, userInfo);
            recordService.Insert(unpublishableRecord, userInfo);
            recordService.Insert(assessedRecordWithUrlResource, userInfo);
            recordService.Insert(assessedRecordWithMixedResources, userInfo);
            recordService.Insert(signedOffUnpublishableRecord, userInfo);
            recordService.Insert(signedOffPublishableUnknownRecord, userInfo);
        }

        void AddRecordWithLotsOfVocablessTags()
        {
            var record = MakeExampleSeedRecord().With(r =>
            {
                r.Id = Helpers.AddCollection("58fbee5e-58e6-4119-82cb-587ec383cb62");
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

            recordService.Insert(record, userInfo);
        }

        void AddRecordsWithSameKeywordsInDifferentVocabs()
        {
            var recordA = MakeExampleSeedRecord().With(r =>
            {
                r.Id = Helpers.AddCollection("b51a21de-b2ac-4a99-9b7e-5c5222d280c1");
                r.Path = @"X:\butterflies\a";
                r.Gemini = r.Gemini.With(m =>
                {
                    m.Title = "This record has a keyword 'butterfly' in vocab 'http://vocab.jncc.gov.uk/seabed-survey-technique'";
                    m.Abstract = "This is just another example record.";
                    m.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/seabed-survey-technique", Value = "butterfly" });
                });
            });

            var recordB = MakeExampleSeedRecord().With(r =>
            {
                r.Id = Helpers.AddCollection("4a3e084b-666b-4c85-ae44-c2c7a266a455");
                r.Path = @"X:\butterflies\b";
                r.Gemini = r.Gemini.With(m =>
                {
                    m.Title = "This record has a keyword 'butterfly' in vocab 'http://vocab.jncc.gov.uk/seabed-survey-purpose'";
                    m.Abstract = "This is just another example record.";
                    m.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/seabed-survey-purpose", Value = "butterfly" });
                });
            });

            recordService.Insert(recordA, userInfo);
            recordService.Insert(recordB, userInfo);
        }

        void AddReadOnlyRecord()
        {
            var record = MakeExampleSeedRecord().With(r =>
                {
                    r.Id = Helpers.AddCollection("b65d2914-cbac-4230-a7f3-08d13eea1e92");
                    r.Path = @"X:\path\to\read\only\record\data";
                    r.ReadOnly = true;
                    r.Gemini = r.Gemini.With(m =>
                        {
                            m.Title = "An example read-only record";
                            m.Abstract = "This is an example read-only record.";
                        });
                });

            recordService.Insert(record, userInfo);
        }

        void AddNonGeographicDataset()
        {
            var record = MakeExampleSeedRecord().With(r =>
            {
                r.Id = Helpers.AddCollection("397a03d6-2770-445f-9900-fdb18850b5f8");
                r.Path = @"X:\path\to\nongeographic\data";
                r.Gemini = r.Gemini.With(m =>
                {
                    m.Title = "An example non-geographic record";
                    m.Abstract = "This is an example non-geographic record.";
                    m.ResourceType = "nonGeographicDataset";
                });
            });

            recordService.Insert(record, userInfo);
        }

        void AddSecureRecords()
        {
            var record = this.MakeExampleSeedRecord().With(r =>
                {
                    r.Id = Helpers.AddCollection("89136d54-d383-4d4d-a385-ac9687596b01");
                    r.Path = @"X:\path\to\restricted\record\data";
                    r.Security = Security.OfficialSensitive;
                    r.Gemini = r.Gemini.With(m =>
                        {
                            m.Title = "An example restricted record";
                            m.Abstract = "This is an example restricted record.";
                        });
                });

            recordService.Insert(record, userInfo);
        }

        void AddNonTopCopyRecord()
        {
            var record = MakeExampleSeedRecord().With(r =>
                {
                    r.Id = Helpers.AddCollection("94f2c217-2e45-42be-8b48-c5075401e508");
                    r.Path = @"X:\path\to\non\top\copy\record\data";
                    r.TopCopy = false;
                    r.Gemini = r.Gemini.With(m =>
                        {
                            m.Title = "An example record that is not top-copy";
                            m.Abstract = "This is an example record that is not top-copy.";
                        });
                });

            recordService.Insert(record, userInfo);
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

                recordService.Insert(record, userInfo);
            }
        }

        void AddRecordWithUnusualCharactersInKeywords()
        {
            var record = MakeExampleSeedRecord().With(r =>
            {
                r.Id = Helpers.AddCollection("1b875458-2c17-44f8-aae0-f6ed9902e70b");
                r.Path = @"X:\path\for\record\with\unusual\characters\in\keywords";
                r.Gemini = r.Gemini.With(m =>
                {
                    m.Title = "A record with unusual characters in keywords";
                });
                // add a keyword with parentheses and hypen
                r.Gemini.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/human-activity", Value = "Extraction � Water (abstraction)" });
                r.Gemini.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/some-vocab", Value = "Two words" });
            });

            recordService.Insert(record, userInfo);
        }

        void AddTwoRecordsWithTheSameBoundingBox()
        {
            var wales = new BoundingBox { North = 53.98783m, South = 50.92033m, East = -2.919512m, West = -5.682273m };

            var record1 = MakeExampleSeedRecord().With(r =>
            {
                r.Id = Helpers.AddCollection("50749e9a-4df5-4641-81a7-1fea04346be5");
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
                r.Id = Helpers.AddCollection("6a969b7f-18e8-4e96-b0ea-371d8e2ba774");
                r.Path = @"X:\path\for\record\with\same\bounding\box\as\another\2";
                r.Gemini = r.Gemini.With(m =>
                {
                    m.Title = "A record with the same bounding box an another (2)";
                    m.BoundingBox = wales;
                });
                r.Gemini.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/some-vocab", Value = "Bounding boxes" });
            });

            recordService.Insert(record1, userInfo);
            recordService.Insert(record2, userInfo);
        }

        void AddBboxes()
        {
            var smallBox = MakeExampleSeedRecord().With(r =>
                {
                    r.Id = Helpers.AddCollection("764dcdea-1231-4494-bc18-6931cc8adcee");
                    r.Path = @"Z:\path\to\small\box";
                    r.Gemini = r.Gemini.With(m =>
                        {
                            m.Title = "Small Box";
                            m.DataFormat = "csv";
                            m.BoundingBox = new BoundingBox { North = 30, South = 20, East = 60, West = 50 };
                        });
                });

           recordService.Insert(smallBox, userInfo);
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

            var meshGui = new Vocabulary
            {
                Id = "http://vocab.jncc.gov.uk/mesh-gui",
                Name = "MESH GUI",
                Description = "Used by MESH",
                PublicationDate = "2013",
                Publishable = false,
                Controlled = true,
                Keywords = new List<VocabularyKeyword>()
            };
            db.Store(meshGui);

            var humanActivity = new Vocabulary
            {
                Id = "http://vocab.jncc.gov.uk/human-activity",
                Name = "Human Activity",
                Description = "Human Activity vocab",
                PublicationDate = "2013",
                Publishable = false,
                Controlled = true,
                Keywords = new List<VocabularyKeyword>()
            };
            db.Store(humanActivity);

            var someVocab = new Vocabulary
            {
                Id = "http://vocab.jncc.gov.uk/some-vocab",
                Name = "Some Vocab",
                Description = "Some Vocab",
                PublicationDate = "2013",
                Publishable = false,
                Controlled = true,
                Keywords = new List<VocabularyKeyword>()
            };
            db.Store(someVocab);

            var anotherVocab = new Vocabulary
            {
                Id = "http://vocab.jncc.gov.uk/another-vocab",
                Name = "Another vocab",
                Description = "Another vocab",
                PublicationDate = "2013",
                Publishable = false,
                Controlled = true,
                Keywords = new List<VocabularyKeyword>()
            };
            db.Store(anotherVocab);
        }

        void AddDatesForTimeline()
        {
            var timeGetter = Clock.CurrentUtcDateTimeGetter;

            var peteUser = new UserInfo
            {
                DisplayName = "Pete",
                Email = "pete@jncc.gov.uk"
            };

            var cathyUser = new UserInfo
            {
                DisplayName = "Cathy",
                Email = "cathy@jncc.gov.uk"
            };

            var felixUser = new UserInfo
            {
                DisplayName = "Felix",
                Email = "felix@jncc.gov.uk"
            };

            Clock.CurrentUtcDateTimeGetter = () => DateTime.Now;
            var timelineTest1 = MakeExampleSeedRecord().With(r =>
            {
                r.Id = Helpers.AddCollection("3671d004-a476-42e6-be1f-ef270784382e");
                r.Path = @"Z:\path\to\timeline\test1";
                r.Gemini = r.Gemini.With(m =>
                {
                    m.Title = "Timeline Test 1";
                    m.MetadataPointOfContact.Name = "Cathy";
                });
                r.Footer = new Footer
                {
                    CreatedOnUtc = Clock.NowUtc.AddDays(-1),
                    CreatedByUser = peteUser,
                    ModifiedOnUtc = Clock.NowUtc,
                    ModifiedByUser = cathyUser
                };
            });
            recordService.Update(timelineTest1, timelineTest1.Footer.ModifiedByUser);

            Clock.CurrentUtcDateTimeGetter = () => DateTime.Now.AddHours(-5);
            var timelineTest2 = MakeExampleSeedRecord().With(r =>
            {
                r.Id = Helpers.AddCollection("e26e1c00-7e2b-47c6-9ca7-bf0fedcfc72c");
                r.Path = @"Z:\path\to\timeline\test2";
                r.Gemini = r.Gemini.With(m =>
                {
                    m.Title = "Timeline Test 2";
                    m.MetadataPointOfContact.Name = "Pete";
                });
                r.Footer = new Footer
                {
                    CreatedOnUtc = Clock.NowUtc,
                    CreatedByUser = peteUser,
                    ModifiedOnUtc = Clock.NowUtc,
                    ModifiedByUser = peteUser
                };
            });
            recordService.Update(timelineTest2, timelineTest2.Footer.ModifiedByUser);

            Clock.CurrentUtcDateTimeGetter = () => DateTime.Now.AddDays(-2);
            var timelineTest3 = MakeExampleSeedRecord().With(r =>
            {
                r.Id = Helpers.AddCollection("71c4e034-0c6b-4fa7-9b4d-ac328d34dabf");
                r.Path = @"Z:\path\to\timeline\test3";
                r.Gemini = r.Gemini.With(m =>
                {
                    m.Title = "Timeline Test 3";
                    m.MetadataPointOfContact.Name = "Felix";
                });
                r.Footer = new Footer
                {
                    CreatedOnUtc = Clock.NowUtc.AddDays(-2),
                    CreatedByUser = felixUser,
                    ModifiedOnUtc = Clock.NowUtc,
                    ModifiedByUser = felixUser
                };
            });
            recordService.Update(timelineTest3, timelineTest3.Footer.ModifiedByUser);

            Clock.CurrentUtcDateTimeGetter = () => DateTime.Now.AddMonths(-4);
            var timelineTest4 = MakeExampleSeedRecord().With(r =>
            {
                r.Id = Helpers.AddCollection("7c0055d1-8076-42f3-a004-ceaf2ad8ae9e");
                r.Path = @"Z:\path\to\timeline\test4";
                r.Gemini = r.Gemini.With(m =>
                {
                    m.Title = "Timeline Test 4";
                    m.MetadataPointOfContact.Name = "Matt";
                });
                r.Footer = new Footer
                {
                    CreatedOnUtc = Clock.NowUtc.AddDays(-5),
                    CreatedByUser = cathyUser,
                    ModifiedOnUtc = Clock.NowUtc,
                    ModifiedByUser = peteUser
                };
            });
            recordService.Update(timelineTest4, timelineTest4.Footer.ModifiedByUser);

            Clock.CurrentUtcDateTimeGetter = timeGetter;
        }

        // BigBoundingBoxWithNothingInside and SmallBox do not intersect
        // verify using http://arthur-e.github.io/Wicket/sandbox-gmaps3.html

        public static readonly string BoundingBoxContainingNothing = "POLYGON((10 10,40 10,40 40,10 40,10 10))";
        public static readonly string BoundingBoxContainingSmallBox = "POLYGON((40 10,60 10,60 30,40 30,40 10))";
    }
}
