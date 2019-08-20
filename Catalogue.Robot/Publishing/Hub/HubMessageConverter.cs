using Catalogue.Data;
using Catalogue.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace Catalogue.Robot.Publishing.Hub
{
    public interface IHubMessageConverter
    {
        string GetS3PublishMessage(string recordId);
        string ConvertRecordToHubPublishMessage(Record record);
    }

    public class HubMessageConverter : IHubMessageConverter
    {
        private readonly Env env;
        private readonly IFileHelper fileHelper;

        public HubMessageConverter(Env env, IFileHelper fileHelper)
        {
            this.env = env;
            this.fileHelper = fileHelper;
        }

        public string GetS3PublishMessage(string recordId)
        {
            var message = new
            {
                config = new
                {
                    s3 = new
                    {
                        bucketName = env.HUB_LARGE_MESSAGE_BUCKET,
                        objectKey = recordId
                    },
                    action = "s3-publish"
                },
                asset = new
                {
                    id = recordId
                }
            };

            return JsonConvert.SerializeObject(message, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        public string ConvertRecordToHubPublishMessage(Record record)
        {
            ConvertEmptyStringsToNull(record);

            var message = new
            {
                config = new
                {
                    elasticsearch = new
                    {
                        index = env.ES_INDEX,
                        site = env.ES_SITE
                    },
                    hub = new
                    {
                        baseUrl = env.HUB_BASE_URL
                    },
                    dynamo = new
                    {
                        table = env.HUB_DYNAMO_TABLE
                    },
                    sqs = new
                    {
                        queueEndpoint = env.SQS_ENDPOINT,
                        largeMessageBucket = env.SQS_PAYLOAD_BUCKET
                    },
                    action = "publish"
                },
                asset = new
                {
                    id = Helpers.RemoveCollection(record.Id),
                    digitalObjectIdentifier = record.DigitalObjectIdentifier,
                    citation = record.Citation,
                    image = record.Image,
                    metadata = new
                    {
                        title = record.Gemini.Title,
                        @abstract = record.Gemini.Abstract,
                        topicCategory = record.Gemini.TopicCategory,
                        keywords = record.Gemini.Keywords,
                        temporalExtent = record.Gemini.TemporalExtent,
                        datasetReferenceDate = record.Gemini.DatasetReferenceDate,
                        lineage = record.Gemini.Lineage,
                        dataFormat = record.Gemini.DataFormat,
                        responsibleOrganisation = record.Gemini.ResponsibleOrganisation,
                        limitationsOnPublicAccess = record.Gemini.LimitationsOnPublicAccess,
                        useConstraints = record.Gemini.UseConstraints,
                        spatialReferenceSystem = record.Gemini.SpatialReferenceSystem,
                        metadataDate = record.Gemini.MetadataDate,
                        metadataPointOfContact = record.Gemini.MetadataPointOfContact,
                        resourceType = record.Gemini.ResourceType,
                        metadataLanguage = "English",
                        boundingBox = record.Gemini.BoundingBox
                    },
                    data = GetData(record)
                }
            };

            return JsonConvert.SerializeObject(message, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        private void ConvertEmptyStringsToNull(Record record)
        {
            // handle empty strings for dynamo, should probably use a custom converter if this gets too big

            if (IsEmptyString(record.DigitalObjectIdentifier))
                record.DigitalObjectIdentifier = null;

            if (IsEmptyString(record.Citation))
                record.Citation = null;

            if (IsEmptyString(record.Gemini.DataFormat))
                record.Gemini.DataFormat = null;

            if (record.Gemini.TemporalExtent != null)
            {
                if (IsEmptyString(record.Gemini.TemporalExtent.Begin))
                    record.Gemini.TemporalExtent.Begin = null;
                if (IsEmptyString(record.Gemini.TemporalExtent.End))
                    record.Gemini.TemporalExtent.End = null;
            }

            if (IsEmptyString(record.Gemini.SpatialReferenceSystem))
                record.Gemini.SpatialReferenceSystem = null;

            foreach (var keyword in record.Gemini.Keywords)
            {
                if (keyword.Vocab != null && string.IsNullOrWhiteSpace(keyword.Vocab))
                    keyword.Vocab = null;
            }
        }

        private bool IsEmptyString(string value)
        {
            return value != null && string.IsNullOrWhiteSpace(value);
        }

        private List<object> GetData(Record record)
        {
            List<object> data = new List<object>();

            if (record.Resources?.Count > 0)
            {
                foreach (var resource in record.Resources)
                {
                    if (fileHelper.IsPdfFile(resource.Path) && !string.IsNullOrWhiteSpace(resource.PublishedUrl))
                    {
                        data.Add(new
                        {
                            title = resource.Name,
                            http = new
                            {
                                url = resource.PublishedUrl,
                                fileBase64 = fileHelper.GetBase64String(resource.Path),
                                fileExtension = fileHelper.GetFileExtensionWithoutDot(resource.Path),
                                fileBytes = fileHelper.GetFileSizeInBytes(resource.Path)
                            }
                        });
                    }
                    else if (Helpers.IsFileResource(resource) && !string.IsNullOrWhiteSpace(resource.PublishedUrl))
                    {
                        data.Add(new
                        {
                            title = resource.Name,
                            http = new
                            {
                                url = resource.PublishedUrl,
                                fileExtension = fileHelper.GetFileExtensionWithoutDot(resource.Path),
                                fileBytes = fileHelper.GetFileSizeInBytes(resource.Path)
                            }
                        });
                    }
                    else if (!Helpers.IsFileResource(resource))
                    {
                        data.Add(new
                        {
                            title = resource.Name,
                            http = new
                            {
                                url = resource.Path
                            }
                        });
                    }
                }
            }

            return data;
        }
    }
}
