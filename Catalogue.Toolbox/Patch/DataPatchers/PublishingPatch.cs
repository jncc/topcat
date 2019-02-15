using Catalogue.Data.Model;
using log4net;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Sparrow.Json;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Catalogue.Toolbox.Patch.DataPatchers
{
    public class PublishingPatch : IDataPatcher
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PublishingPatch));

        readonly IDocumentStore store;

        public PublishingPatch()
        {
            store = new DocumentStore
            {
                Urls = new[] { ConfigurationManager.AppSettings["RavenDbUrls"] },
                Database = ConfigurationManager.AppSettings["RavenDbDatabase"]
            };

            var old = store.Conventions.DeserializeEntityFromBlittable;
            store.Conventions.DeserializeEntityFromBlittable = (type, doc) =>
            {
                var entity = old(type, doc);
                if (entity is Record record)
                {
                    Logger.Info($"Checking fields for record {record.Id}");

                    MigrateResourceLocator(doc, record);
                    MigrateOpenDataInfo(doc, record);
                }
                return entity;
            };
            store.Conventions.PreserveDocumentPropertiesNotFoundOnModel = false;

            store.Initialize();
            IndexCreation.CreateIndexes(typeof(Record).Assembly, store);
        }

        public void Patch()
        {
            using (var db = store.OpenSession())
            {
                Logger.Info("Running publishing patch");

                var records = db
                    .Query<Record>()
                    .Skip(0)
                    .Take(5000)
                    .ToList();

                Logger.Info($"Checked {records.Count} records for migration");
                db.SaveChanges();
            }
        }

        private static void MigrateOpenDataInfo(BlittableJsonReaderObject doc, Record record)
        {
            if (doc.TryGetMember("Publication", out object publication))
            {
                if (publication != null)
                {
                    if (publication is BlittableJsonReaderObject jsonObject)
                    {
                        if (jsonObject.TryGet("OpenData", out object openData))
                        {
                            if (openData != null)
                            {
                                if (openData is BlittableJsonReaderObject openDataObject)
                                {
                                    MigrateAssessmentInfo(openDataObject, record);
                                    MigrateSignOffInfo(openDataObject, record);
                                    MigratePublishableStatus(openDataObject, record);
                                    MigrateResources(openDataObject, record);
                                    MigrateLastAttempt(openDataObject, record);
                                    MigrateLastSuccess(openDataObject, record);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void MigrateLastSuccess(BlittableJsonReaderObject openDataObject, Record record)
        {
            if (openDataObject.TryGet("LastSuccess", out PublicationAttempt lastSuccess))
            {
                if (lastSuccess != null)
                {
                    Logger.Info("Migrating last success info");

                    if (record.Publication.Data == null)
                    {
                        record.Publication.Data = new DataInfo();
                    }

                    if (record.Publication.Target == null)
                    {
                        record.Publication.Target = new TargetInfo();
                    }

                    if (record.Publication.Target.Gov == null)
                    {
                        record.Publication.Target.Gov = new GovPublicationInfo();
                    }

                    record.Publication.Data.LastSuccess = lastSuccess;
                    record.Publication.Target.Gov.LastSuccess = lastSuccess;
                }
            }
        }

        private static void MigrateLastAttempt(BlittableJsonReaderObject openDataObject, Record record)
        {
            if (openDataObject.TryGet("LastAttempt", out PublicationAttempt lastAttempt))
            {
                if (lastAttempt != null)
                {
                    Logger.Info("Migrating last attempt info");

                    if (record.Publication.Data == null)
                    {
                        record.Publication.Data = new DataInfo();
                    }

                    if (record.Publication.Target == null)
                    {
                        record.Publication.Target = new TargetInfo();
                    }

                    if (record.Publication.Target.Gov == null)
                    {
                        record.Publication.Target.Gov = new GovPublicationInfo();
                    }

                    record.Publication.Data.LastAttempt = lastAttempt;
                    record.Publication.Target.Gov.LastAttempt = lastAttempt;
                }
            }
        }

        private static void MigrateSignOffInfo(BlittableJsonReaderObject openDataObject, Record record)
        {
            if (openDataObject.TryGet("SignOff", out SignOffInfo signOff))
            {
                Logger.Info("Migrating sign off info");
                record.Publication.SignOff = signOff;
            }
        }

        private static void MigrateAssessmentInfo(BlittableJsonReaderObject openDataObject, Record record)
        {
            if (openDataObject.TryGetMember("Assessment", out object assessment))
            {
                if (assessment != null)
                {
                    Logger.Info("Migrating assessment info");
                    if (assessment is BlittableJsonReaderObject assessmentObject)
                    {
                        record.Publication.Assessment = new AssessmentInfo();
                        
                        if (assessmentObject.TryGet("Completed", out bool completed))
                        {
                            record.Publication.Assessment.Completed = completed;
                        }
                    }
                }
            }
        }

        private static void MigrateResources(BlittableJsonReaderObject openDataObject, Record record)
        {
            if (openDataObject.TryGet("Resources", out List<Resource> resources))
            {
                if (resources != null)
                {
                    Logger.Info("Migrating resources");

                    if (record.Publication.Data == null)
                    {
                        record.Publication.Data = new DataInfo();
                    }

                    if (record.Publication.Data.Resources == null)
                    {
                        record.Publication.Data.Resources = new List<Resource>();
                    }

                    foreach (var resource in resources)
                    {
                        if (string.IsNullOrWhiteSpace(resource.Name))
                        {
                            record.Publication.Data.Resources.Add(new Resource
                            {
                                Name = resource.Path,
                                Path = resource.Path
                            });
                        }
                        else
                        {
                            record.Publication.Data.Resources.Add(new Resource
                            {
                                Name = resource.Name,
                                Path = resource.Path
                            });
                        }
                    }
                }
            }
        }

        private static void MigratePublishableStatus(BlittableJsonReaderObject openDataObject, Record record)
        {
            if (openDataObject.TryGet("Publishable", out bool? publishable))
            {
                Logger.Info("Migrating publishable status");

                if (record.Publication.Target == null)
                {
                    record.Publication.Target = new TargetInfo();
                }

                if (record.Publication.Target.Hub == null)
                {
                    record.Publication.Target.Hub = new HubPublicationInfo();
                }

                if (record.Publication.Target.Gov == null)
                {
                    record.Publication.Target.Gov = new GovPublicationInfo();
                }

                record.Publication.Target.Hub.Publishable = publishable;
                record.Publication.Target.Gov.Publishable = publishable;
            }
        }

        private static void MigrateResourceLocator(BlittableJsonReaderObject doc, Record record)
        {
            if (doc.TryGetMember("Gemini", out object gemini))
            {
                if (gemini is BlittableJsonReaderObject jsonObject)
                {
                    if (jsonObject.TryGet("ResourceLocator", out string resourceLocator))
                    {
                        if (!string.IsNullOrWhiteSpace(resourceLocator))
                        {
                            Logger.Info($"Migrating resource locator {resourceLocator}");

                            if (record.Publication == null)
                            {
                                record.Publication = new PublicationInfo();
                            }

                            if (record.Publication.Data == null)
                            {
                                record.Publication.Data = new DataInfo();
                            }

                            if (record.Publication.Data.Resources == null)
                            {
                                record.Publication.Data.Resources = new List<Resource>();
                            }

                            record.Publication.Data.Resources.Add(new Resource
                            {
                                Name = resourceLocator,
                                Path = resourceLocator
                            });
                        }
                    }
                }
            }
        }
    }
}
