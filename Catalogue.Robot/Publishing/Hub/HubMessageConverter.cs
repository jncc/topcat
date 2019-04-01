using Catalogue.Data;
using Catalogue.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace Catalogue.Robot.Publishing.Hub
{
    public interface IHubMessageConverter
    {
        string ConvertRecordToHubAsset(Record record);
        string ConvertRecordToQueueMessage(Record record);
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

        public string ConvertRecordToHubAsset(Record record)
        {
            ConvertEmptyStringsToNull(record);

            var asset = new
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
            };

            return JsonConvert.SerializeObject(asset, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        public string ConvertRecordToQueueMessage(Record record)
        {
            var messageResources = new List<object>();

            if (record.Publication.Data?.Resources?.Count > 0)
            {
                foreach (var resource in record.Publication.Data.Resources)
                {
                    if (IsPdfFileResource(resource) && !string.IsNullOrWhiteSpace(resource.PublishedUrl))
                    {
                        messageResources.Add(new
                        {
                            title = resource.Name,
                            content = record.Gemini.Abstract,
                            url = resource.PublishedUrl,
                            file_base64 = fileHelper.GetBase64String(resource.Path),
                            file_extension = fileHelper.GetFileExtensionWithoutDot(resource.Path),
                            file_bytes = fileHelper.GetFileSizeInBytes(resource.Path)
                        });
                    }
                    else if (Helpers.IsFileResource(resource) && !string.IsNullOrWhiteSpace(resource.PublishedUrl))
                    {
                        messageResources.Add(new
                        {
                            title = resource.Name,
                            content = record.Gemini.Abstract,
                            url = resource.PublishedUrl,
                            file_extension = fileHelper.GetFileExtensionWithoutDot(resource.Path),
                            file_bytes = fileHelper.GetFileSizeInBytes(resource.Path)
                        });
                    }
                }
            }

            var message = new
            {
                verb = "upsert",
                index = env.HUB_QUEUE_INDEX,
                document = new
                {
                    id = Helpers.RemoveCollection(record.Id),
                    site = "datahub",
                    title = record.Gemini.Title,
                    content = record.Gemini.Abstract,
                    url = record.Publication.Target.Hub.Url,
                    resource_type = record.Gemini.ResourceType,
                    keywords = record.Gemini.Keywords,
                    published_date = record.Gemini.DatasetReferenceDate
                },
                resources = messageResources
            };

            return JsonConvert.SerializeObject(message, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
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

            if (record.Publication.Data?.Resources?.Count > 0)
            {
                foreach (var resource in record.Publication.Data.Resources)
                {
                    if (Helpers.IsFileResource(resource) && !string.IsNullOrWhiteSpace(resource.PublishedUrl))
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

        private bool IsPdfFileResource(Resource resource)
        {
            if (Helpers.IsFileResource(resource))
            {
                var extension = fileHelper.GetFileExtensionWithoutDot(resource.Path);

                if (!string.IsNullOrWhiteSpace(extension) && extension.Equals("pdf"))
                {
                    return true;
                }
            }
            
            return false;
        }
    }

}
