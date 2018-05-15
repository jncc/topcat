﻿using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Catalogue.Data.Query;
using Catalogue.Data.Write;
using Catalogue.Gemini.BoundingBoxes;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.Spatial;
using Raven.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Catalogue.Data.Import.Mappings;
using Catalogue.Utilities.Time;
using CsvHelper;

namespace Catalogue.Web.Controllers.Patch
{
    public class PatchController : ApiController
    {
        readonly IDocumentSession db;
        readonly IRecordQueryer _queryer;

        public PatchController(IDocumentSession db, IRecordQueryer _queryer)
        {
            this.db = db;
            this._queryer = _queryer;
        }

        [HttpPost, Route("api/patch/fixupkeywords")]
        public HttpResponseMessage FixUpKeywords()
        {
            // protected sites
            var query2 = new RecordQueryInputModel
            {
                F = new FilterOptions{Keywords = new[] { "vocab.jncc.gov.uk/jncc-broad-category/Marine Protected Sites" }},
                P = 0,
                N = 1024,
            };

            var records2 = _queryer.Query(query2).ToList();

            foreach (var record in records2)
            {
                var existing = record.Gemini.Keywords.Single(k => k.Vocab == "http://vocab.jncc.gov.uk/jncc-broad-category" && k.Value == "Marine Protected Sites");
                record.Gemini.Keywords.Remove(existing);

                var domain = new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-domain", Value = "Marine" };
                var category = new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-category", Value = "Protected Areas" };

                record.Gemini.Keywords.Insert(0, domain);
                record.Gemini.Keywords.Insert(1, category);
            }


            db.SaveChanges();

            return new HttpResponseMessage();
        }

        [HttpPost, Route("api/patch/fixupvocabs")]
        public HttpResponseMessage FixUpVocabs()
        {
            var broad = db.Load<Vocabulary>("http://vocab.jncc.gov.uk/jncc-broad-category");

            db.Delete(broad);

            var domain = new Vocabulary
            {
                Id = "http://vocab.jncc.gov.uk/jncc-domain",
                Name = "JNCC Domain",
                Description = "The broad domain within JNCC.",
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
            db.Store(domain);

            var category = new Vocabulary
            {
                Id = "http://vocab.jncc.gov.uk/jncc-category",
                Name = "JNCC Category",
                Description = "The data category within JNCC.",
                PublicationDate = "2015",
                Publishable = true,
                Controlled = true,
                Keywords = new List<VocabularyKeyword>
                        {
                            new VocabularyKeyword { Value = "Seabed Habitat Maps" },
                            new VocabularyKeyword { Value = "Protected Areas" },
                        }
            };
            db.Store(category);

            db.SaveChanges();
            return new HttpResponseMessage();
        }

        [HttpPost, Route("api/patch/fixupactivitiesemailaddress")]
        public HttpResponseMessage FixUpActivitiesEmailAddress()
        {
            var query = new RecordQueryInputModel
            {
                F = new FilterOptions{Keywords = new[] { "vocab.jncc.gov.uk/jncc-category/Human Activities" }},
                P = 0,
                N = 1024,
            };

            var records = _queryer.Query(query).ToList();

            foreach (var record in records)
            {
                record.Gemini.MetadataPointOfContact.Email = record.Gemini.MetadataPointOfContact.Email.Replace("jnccc", "jncc");
            }

            db.SaveChanges();

            return new HttpResponseMessage();
        }

        [HttpPost, Route("api/patch/addotboundingboxes")]
        public HttpResponseMessage AddOTBoundingBoxes()
        {
            var query = new RecordQueryInputModel
            {
                F = new FilterOptions{Keywords = new[] { "vocab.jncc.gov.uk/jncc-category/Overseas Territories" }},
                P = 0,
                N = 1024,
            };

            var records = _queryer.Query(query).ToList();

            foreach (var record in records)
            {
                // note incorrect 's' in vocabs.jncc!
                var ots = record.Gemini.Keywords.Where(k => k.Vocab == "http://vocabs.jncc.gov.uk/overseas-territories").ToList();

                if (ots.Any())
                {
                    var allBoxes = BoundingBoxes.Groups.Single(g => g.Name == "Overseas Territories").Boxes
                        .Concat(new[] // currently don't have these so just use 0,0,0,0
                        {
                            new KnownBoundingBox { Name = "Sovereign Base Areas Cyprus", Box = new BoundingBox() },
                            new KnownBoundingBox { Name = "British Antarctic Territory", Box = new BoundingBox() }, 
                        });

                    var boxes = ots.Select(ot => allBoxes.SingleOrDefault(b => b.Name == ot.Value)).ToList();

                    if (boxes.Any(b => b == null))
                        throw new Exception("Some box couldn't be looked up for record " + record.Id);

                    var box = BoundingBoxUtility.MinimumOf(boxes.Select(b => b.Box));
                    record.Gemini.BoundingBox = box;
                }
            }

            db.SaveChanges();

            return new HttpResponseMessage();
        }

        [HttpGet, Route("api/patch/createhumanactiityvocabrecord")]
        public List<string> CreateHumanActiityVocabRecord()
        {
            var recordKeywords = db.Query<RecordKeywordIndex.Result, RecordKeywordIndex>()
                .Where(x => x.Vocab == "http://vocab.jncc.gov.uk/human-activity")
                .Select(x => x.Value )
                .ToList();

            return recordKeywords;
        }

        [HttpPost, Route("api/patch/resave")]
        public HttpResponseMessage Resave(string category)
        {
            var query = new RecordQueryInputModel
            {
                F = new FilterOptions{Keywords = new[] { "vocab.jncc.gov.uk/jncc-category/" + category }},
                P = 0,
                N = 1024,
            };

            var records = _queryer.Query(query).ToList();

            var service = new RecordService(db, new RecordValidator());

            foreach (var record in records)
            {
                service.Update(record, record.Footer.ModifiedByUser);
            }

            db.SaveChanges();

            return new HttpResponseMessage();
        }

        [HttpPost, Route("api/patch/fixseabedsurvey")]
        public HttpResponseMessage FixSeabedSurvey()
        {
            var query = new RecordQueryInputModel
            {
                F = new FilterOptions{Keywords = new[] { "vocab.jncc.gov.uk/jncc-category/Offshore Seabed Survey" }},
                P = 0,
                N = 1024,
            };

            var records = _queryer.Query(query).ToList();

            var service = new RecordService(db, new RecordValidator());

            foreach (var record in records)
            {
                record.Gemini.ResponsibleOrganisation.Role = "custodian";
                service.Update(record, record.Footer.ModifiedByUser);
            }


            db.SaveChanges();

            return new HttpResponseMessage { Content = new StringContent("Updated " + records.Count + " records.") };
        }

        [HttpPost, Route("api/patch/migrateopendatapublicationinfo")]
        public HttpResponseMessage MigrateOpenDataPublicationInfo()
        {
            var records1 = db
                .Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>()
                .As<Record>()
                .Skip(0)
                .Take(1024)
                .ToList();

            var records2 = db
                .Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>()
                .As<Record>()
                .Skip(1024)
                .Take(1024)
                .ToList();

            var records = records1.Concat(records2).ToList();

            foreach (var record in records)
            {
                var old = record.Publication.OpenData;

                if (old.SignOff == null)
                {
                    record.Publication.OpenData = new OpenDataPublicationInfo
                    {
                        Assessment = new OpenDataAssessmentInfo { Completed = true, InitialAssessmentWasDoneOnSpreadsheet = true },
                        SignOff = new OpenDataSignOffInfo(),
                        LastAttempt = old.LastAttempt,
                        LastSuccess = old.LastSuccess,
                        Paused = old.Paused,
                        Resources = old.Resources,
                    };
                }
            }

            db.SaveChanges();

            return new HttpResponseMessage { Content = new StringContent("Updated " + records.Count + " records.") };

        }

        [HttpPost, Route("api/patch/republishredactions")]
        public HttpResponseMessage RepublishRedactions()
        {
            var records1 = db
                .Query<Record>()
                .As<Record>()
                .Skip(0)
                .Take(1024)
                .ToList();

            var records2 = db
                .Query<Record>()
                .As<Record>()
                .Skip(1024)
                .Take(1024)
                .ToList();

            var records3 = db
                .Query<Record>()
                .As<Record>()
                .Skip(2048)
                .Take(1024)
                .ToList();

            var records4 = db
                .Query<Record>()
                .As<Record>()
                .Skip(3072)
                .Take(1024)
                .ToList();

            var records = records1.Concat(records2).Concat(records3).Concat(records4).ToList();

            var timestamp = Clock.NowUtc;
            var signOffUser = new UserInfo
            {
                DisplayName = "Sonia Khela",
                Email = "Sonia.Khela@jncc.gov.uk"
            };

            var useContraintsMapping = InstantiateMapping("UseConstraintsMapping");
            var metadataContactMapping = InstantiateMapping("MetadataContactMapping");
            using (var useConstraintsReader = new StreamReader("C:\\temp\\TOPCAT_297.csv"))
            using (var metadataReader = new StreamReader("C:\\temp\\TOPCAT_299.csv"))
            {
                var useConstraintsCsv = new CsvReader(useConstraintsReader);
                var metadataCsv = new CsvReader(metadataReader);

                useContraintsMapping.Apply(useConstraintsCsv.Configuration);
                metadataContactMapping.Apply(metadataCsv.Configuration);

                var useConstraintsRecords = useConstraintsCsv.GetRecords<Record>().ToList();
                var metadataContactRecords = metadataCsv.GetRecords<Record>().ToList();

                foreach (var record in records)
                {
                    foreach (var useConstraintsRecord in useConstraintsRecords)
                    {
                        if (record.Id.Equals(useConstraintsRecord.Id))
                        {
                            record.Gemini.LimitationsOnPublicAccess = useConstraintsRecord.Gemini.LimitationsOnPublicAccess;
                            record.Gemini.UseConstraints = useConstraintsRecord.Gemini.UseConstraints;
                        }
                    }

                    foreach (var metadataContactRecord in metadataContactRecords)
                    {
                        if (record.Id.Equals(metadataContactRecord.Id))
                        {
                            if (record.Gemini.MetadataPointOfContact != null)
                            {
                                record.Gemini.MetadataPointOfContact.Name =
                                    metadataContactRecord.Gemini.MetadataPointOfContact.Name;
                                record.Gemini.MetadataPointOfContact.Email =
                                    metadataContactRecord.Gemini.MetadataPointOfContact.Email;
                            }
                            else
                            {
                                record.Gemini.MetadataPointOfContact =
                                    metadataContactRecord.Gemini.MetadataPointOfContact;
                            }

                            if (record.Manager != null)
                            {
                                record.Manager.DisplayName = metadataContactRecord.Manager.DisplayName;
                            }
                            else
                            {
                                record.Manager = new UserInfo
                                {
                                    DisplayName = metadataContactRecord.Manager.DisplayName
                                };
                            }
                        }
                    }

                    if (record.Publication?.OpenData?.LastSuccess != null && record.Publication?.OpenData?.Publishable == true && record.Validation == Validation.Gemini && record.Publication?.OpenData?.Paused == false)
                    {
                        if (record.Gemini.ResponsibleOrganisation != null && record.Gemini.ResponsibleOrganisation.Name == "Joint Nature Conservation Committee (JNCC)" && string.IsNullOrEmpty(record.Gemini.ResponsibleOrganisation.Role))
                        {
                            record.Gemini.ResponsibleOrganisation.Role = "custodian";
                        }

                        record.Gemini.MetadataDate = timestamp;
                        record.Publication.OpenData.Assessment.CompletedByUser = signOffUser;
                        record.Publication.OpenData.Assessment.CompletedOnUtc = timestamp.AddMinutes(-1);
                        record.Publication.OpenData.SignOff.User = signOffUser;
                        record.Publication.OpenData.SignOff.DateUtc = timestamp;
                    }
                }
            }

            db.SaveChanges();

            return new HttpResponseMessage { Content = new StringContent("Updated " + records.Count + " records.") };
        }

        IMapping InstantiateMapping(string mapper)
        {
            var type = typeof(IMapping).Assembly.GetType("Catalogue.Data.Import.Mappings." + mapper);

            if (type == null)
                throw new Exception(String.Format("The import mapping '{0}' couldn't be found or does not exist.", mapper));

            return (IMapping)Activator.CreateInstance(type);
        }
    }
}
