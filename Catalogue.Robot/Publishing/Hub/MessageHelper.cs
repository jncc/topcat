using System.Collections.Generic;
using Catalogue.Data;
using Catalogue.Data.Model;
using Newtonsoft.Json;

namespace Catalogue.Robot.Publishing.Hub
{
    public class MessageHelper
    {
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
