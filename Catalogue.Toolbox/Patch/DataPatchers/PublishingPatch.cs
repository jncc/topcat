using System;
using Catalogue.Data.Model;
using log4net;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Sparrow.Json;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Catalogue.Data;

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

                    MigrateOpenDataInfo(doc, record);
                    MigrateResourceLocator(doc, record);
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
            if (openDataObject.TryGetMember("LastSuccess", out object lastSuccess))
            {
                if (lastSuccess != null)
                {
                    Logger.Info("Migrating last success info");

                    if (record.Publication.Data == null)
                    {
                        record.Publication.Data = new DataInfo();
                    }

                    if (record.Publication.Data.LastSuccess == null)
                    {
                        record.Publication.Data.LastSuccess = new PublicationAttempt();
                    }

                    if (record.Publication.Target == null)
                    {
                        record.Publication.Target = new TargetInfo();
                    }

                    if (record.Publication.Target.Gov == null)
                    {
                        record.Publication.Target.Gov = new GovPublicationInfo();
                    }

                    if (record.Publication.Target.Gov.LastSuccess == null)
                    {
                        record.Publication.Target.Gov.LastSuccess = new PublicationAttempt();
                    }

                    if (lastSuccess is BlittableJsonReaderObject lastSuccessObject)
                    {
                        if (lastSuccessObject.TryGet("DateUtc", out DateTime datetime))
                        {
                            record.Publication.Data.LastSuccess.DateUtc = datetime;
                            record.Publication.Target.Gov.LastSuccess.DateUtc = datetime;
                        }

                        if (lastSuccessObject.TryGet("Message", out string message))
                        {
                            record.Publication.Data.LastSuccess.Message = message;
                            record.Publication.Target.Gov.LastSuccess.Message = message;
                        }
                    }
                }
            }
        }

        private static void MigrateLastAttempt(BlittableJsonReaderObject openDataObject, Record record)
        {
            if (openDataObject.TryGetMember("LastAttempt", out object lastAttempt))
            {
                if (lastAttempt != null)
                {
                    Logger.Info("Migrating last attempt info");

                    if (record.Publication.Data == null)
                    {
                        record.Publication.Data = new DataInfo();
                    }

                    if (record.Publication.Data.LastAttempt == null)
                    {
                        record.Publication.Data.LastAttempt = new PublicationAttempt();
                    }

                    if (record.Publication.Target == null)
                    {
                        record.Publication.Target = new TargetInfo();
                    }

                    if (record.Publication.Target.Gov == null)
                    {
                        record.Publication.Target.Gov = new GovPublicationInfo();
                    }

                    if (record.Publication.Target.Gov.LastAttempt == null)
                    {
                        record.Publication.Target.Gov.LastAttempt = new PublicationAttempt();
                    }
                    
                    if (lastAttempt is BlittableJsonReaderObject lastAttemptObject)
                    {
                        if (lastAttemptObject.TryGet("DateUtc", out DateTime datetime))
                        {
                            record.Publication.Data.LastAttempt.DateUtc = datetime;
                            record.Publication.Target.Gov.LastAttempt.DateUtc = datetime;
                        }

                        if (lastAttemptObject.TryGet("Message", out string message))
                        {
                            record.Publication.Data.LastAttempt.Message = message;
                            record.Publication.Target.Gov.LastAttempt.Message = message;
                        }
                    }
                }
            }
        }

        private static void MigrateSignOffInfo(BlittableJsonReaderObject openDataObject, Record record)
        {
            if (openDataObject.TryGetMember("SignOff", out object signOff))
            {
                if (signOff != null)
                {
                    Logger.Info("Migrating sign off info");
                    if (signOff is BlittableJsonReaderObject signOffObject)
                    {
                        record.Publication.SignOff = new SignOffInfo();

                        if (signOffObject.TryGetMember("User", out object user))
                        {
                            if (user != null)
                            {
                                if (user is BlittableJsonReaderObject userObject)
                                {
                                    record.Publication.SignOff.User = new UserInfo();

                                    if (userObject.TryGet("DisplayName", out string name))
                                    {
                                        record.Publication.SignOff.User.DisplayName = name;
                                    }

                                    if (userObject.TryGet("Email", out string email))
                                    {
                                        record.Publication.SignOff.User.Email = email;
                                    }
                                }
                            }
                        }

                        if (signOffObject.TryGet("DateUtc", out DateTime datetime))
                        {
                            record.Publication.SignOff.DateUtc = datetime;
                        }

                        if (signOffObject.TryGet("Comment", out string comment))
                        {
                            record.Publication.SignOff.Comment = comment;
                        }
                    }
                }
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

                        if (assessmentObject.TryGetMember("CompletedByUser", out object user))
                        {
                            if (user != null)
                            {
                                if (user is BlittableJsonReaderObject userObject)
                                {
                                    record.Publication.Assessment.CompletedByUser = new UserInfo();

                                    if (userObject.TryGet("DisplayName", out string name))
                                    {
                                        record.Publication.Assessment.CompletedByUser.DisplayName = name;
                                    }

                                    if (userObject.TryGet("Email", out string email))
                                    {
                                        record.Publication.Assessment.CompletedByUser.Email = email;
                                    }
                                }
                            }
                        }

                        if (assessmentObject.TryGet("CompletedOnUtc", out DateTime datetime))
                        {
                            record.Publication.Assessment.CompletedOnUtc = datetime;
                        }

                        if (assessmentObject.TryGet("InitialAssessmentWasDoneOnSpreadsheet", out bool spreadsheetAssessment))
                        {
                            record.Publication.Assessment.InitialAssessmentWasDoneOnSpreadsheet = spreadsheetAssessment;
                        }
                    }
                }
            }
        }

        private static void MigrateResources(BlittableJsonReaderObject openDataObject, Record record)
        {
            if (openDataObject.TryGetMember("Resources", out object resources))
            {
                if (resources != null)
                {
                    if (resources is BlittableJsonReaderArray resourcesArray)
                    {
                        Logger.Info("Migrating resources");

                        if (resourcesArray.Length > 0)
                        {
                            if (record.Resources == null)
                            {
                                record.Resources = new List<Resource>();
                            }

                            foreach (var resource in resourcesArray)
                            {
                                if (resource is BlittableJsonReaderObject resourceObject)
                                {
                                    if (resourceObject.TryGet("Path", out string path))
                                    {
                                        if (!record.Resources.Any(r => r.Path.Equals(path)))
                                        {
                                            var newResource = new Resource();
                                            newResource.Path = path;

                                            if (resourceObject.TryGet("Name", out string name))
                                            {
                                                newResource.Name = name;
                                            }
                                            else
                                            {
                                                newResource.Name = Path.GetFileName(path);
                                            }

                                            if (resourceObject.TryGet("PublishedUrl", out string publishedUrl))
                                            {
                                                newResource.PublishedUrl = publishedUrl;
                                            }

                                            record.Resources.Add(newResource);
                                        }
                                    }
                                }
                            }
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

                if (record.Publication.Target.Gov == null)
                {
                    record.Publication.Target.Gov = new GovPublicationInfo();
                }

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

                            if (record.Resources == null)
                            {
                                record.Resources = new List<Resource>();
                            }

                            if (!record.Resources.Any(r => r.Path.Equals(resourceLocator)))
                            {
                                if (resourceLocator.Contains("http://data.jncc.gov.uk/data/"))
                                {
                                    var friendlyFilename = resourceLocator.Replace(
                                        "http://data.jncc.gov.uk/data/" + Helpers.RemoveCollection(record.Id) + "-",
                                        "");
                                    record.Resources.Add(new Resource
                                    {
                                        Name = friendlyFilename,
                                        Path = resourceLocator,
                                        PublishedUrl = resourceLocator
                                    });
                                }
                                else
                                {
                                    record.Resources.Add(new Resource
                                    {
                                        Name = "Published location for online access",
                                        Path = resourceLocator
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
