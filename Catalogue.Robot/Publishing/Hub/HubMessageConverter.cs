using Catalogue.Data;
using Catalogue.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace Catalogue.Robot.Publishing.Hub
{
    public class HubMessageConverter : IHubMessageConverter
    {
        private readonly IFileHelper fileHelper;

        public HubMessageConverter(IFileHelper fileHelper)
        {
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
                    else if (!Helpers.IsFileResource(resource))
                    {
                        messageResources.Add(new
                        {
                            title = resource.Name,
                            content = record.Gemini.Abstract,
                            url = resource.Path
                        });
                    }
                }
            }

            var message = new
            {
                verb = "upsert",
                index = "topcatdev",
                document = new
                {
                    id = Helpers.RemoveCollection(record.Id),
                    site = "datahub", // as opposed to datahub|sac|mhc
                    title = record.Gemini.Title,
                    content = record.Gemini.Abstract,
                    url = record.Publication.Target.Hub.Url, // the URL of the page, for clicking through
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

            if (record.DigitalObjectIdentifier != null && string.IsNullOrWhiteSpace(record.DigitalObjectIdentifier))
            {
                record.DigitalObjectIdentifier = null;
            }

            if (record.Citation != null && string.IsNullOrWhiteSpace(record.Citation))
            {
                record.Citation = null;
            }

            if (record.Gemini.TemporalExtent?.End != null &&
                string.IsNullOrWhiteSpace(record.Gemini.TemporalExtent.End))
            {
                record.Gemini.TemporalExtent.End = null;
            }

            foreach (var keyword in record.Gemini.Keywords)
            {
                if (keyword.Vocab != null && string.IsNullOrWhiteSpace(keyword.Vocab))
                {
                    keyword.Vocab = null;
                }
            }
        }

        private List<object> GetData(Record record)
        {
            List<object> data = null;

            if (record.Publication.Data?.Resources?.Count > 0)
            {
                data = new List<object>();

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
