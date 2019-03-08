using System.Collections.Generic;
using System.Reflection;
using Catalogue.Data;
using Catalogue.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Catalogue.Robot.Publishing.Hub
{
    public class MessageHelper
    {
        public string ConvertRecordToHubAsset(Record record)
        {
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

        private List<object> GetData(Record record)
        {
            List<object> data = null;

            if (record.Publication.Data?.Resources?.Count > 0)
            {
                data = new List<object>();

                foreach (var resource in record.Publication.Data.Resources)
                {
                    data.Add(new
                    {
                        title = resource.Name,
                        http = new
                        {
                            url = resource.PublishedUrl,
                            fileExtension = Helpers.IsFileResource(resource) ? FileHelper.GetFileExtensionWithoutDot(resource.Path) : null,
                            fileBytes = Helpers.IsFileResource(resource) ? FileHelper.GetFileSizeInBytes(resource.Path) : 0
                        }
                    });
                }
            }

            return data;
        }

        public string ConvertRecordToQueueMessage(Record record)
        {
            var messageResources = new List<object>();

            foreach (var resource in record.Publication.Data.Resources)
            {
                if (IsPdfFileResource(resource) && !string.IsNullOrWhiteSpace(resource.PublishedUrl))
                {
                    messageResources.Add(new
                    {
                        title = resource.Name,
                        content = record.Gemini.Abstract,
                        url = resource.PublishedUrl,
                        file_base64 = FileHelper.GetBase64String(resource.Path),
                        file_extension = FileHelper.GetFileExtensionWithoutDot(resource.Path),
                        file_bytes = FileHelper.GetFileSizeInBytes(resource.Path)
                    });
                }
                else if (Helpers.IsFileResource(resource) && !string.IsNullOrWhiteSpace(resource.PublishedUrl))
                {
                    messageResources.Add(new
                    {
                        title = resource.Name,
                        content = record.Gemini.Abstract,
                        url = resource.PublishedUrl,
                        file_extension = FileHelper.GetFileExtensionWithoutDot(resource.Path),
                        file_bytes = FileHelper.GetFileSizeInBytes(resource.Path)
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

            return JsonConvert.SerializeObject(message, Formatting.None);
        }

        private bool IsPdfFileResource(Resource resource)
        {
            if (Helpers.IsFileResource(resource))
            {
                var extension = FileHelper.GetFileExtensionWithoutDot(resource.Path);

                if (!string.IsNullOrWhiteSpace(extension) && extension.Equals("pdf"))
                {
                    return true;
                }
            }
            
            return false;
        }
    }

}
