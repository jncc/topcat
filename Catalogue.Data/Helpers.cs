using System;
using System.Collections.Generic;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;

namespace Catalogue.Data
{
    public static class Helpers
    {
        public static string AddCollection(string id)
        {
            return "records/" + id;
        }

        public static string RemoveCollection(string id)
        {
            return id.Replace("records/", "");
        }

        public static Record AddCollectionToId(Record record)
        {
            record.Id = "records/" + record.Id;
            return record;
        }

        public static Record RemoveCollectionFromId(Record record)
        {
            record.Id = record.Id.Replace("records/", "");
            return record;
        }

        public static bool IsFileResource(Resource resource)
        {
            var isFilePath = false;
            if (Uri.TryCreate(resource.Path, UriKind.Absolute, out var uri))
            {
                if (uri.Scheme == Uri.UriSchemeFile)
                {
                    isFilePath = true;
                }
            }

            return isFilePath;
        }

        public static List<OnlineResource> GetOnlineResourcesFromDataResources(Record record)
        {
            var resources = new List<OnlineResource>();
            if (record.Resources?.Count > 0)
            {
                foreach (var resource in record.Resources)
                {
                    var url = IsFileResource(resource) ? resource.PublishedUrl : resource.Path;
                    resources.Add(new OnlineResource
                    {
                        Name = resource.Name,
                        Url = url
                    });
                }
            }

            return resources;
        }
    }
}