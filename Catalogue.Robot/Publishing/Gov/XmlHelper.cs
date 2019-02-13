﻿using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Clone;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Quartz.Util;

namespace Catalogue.Robot.Publishing.Gov
{
    public class XmlHelper : IXmlHelper
    {
        public byte[] GetMetadataDocument(Record record)
        {
            // use either the resourcehub url or the direct data links
            var onlineResources = GetOnlineResources(record);

            // redact the record for open data publishing (for GDPR etc.)
            var redacted = GetRedactedRecordToPublish(record);

            // generate the XML
            var doc = new Gemini.Encoding.XmlEncoder().Create(redacted.Id, redacted.Gemini, onlineResources);

            var s = new MemoryStream();
            doc.Save(s);

            return s.ToArray();
        }

        static Record GetRedactedRecordToPublish(Record record)
        {
            // create a *clone* of the record with redacted properties
            // (because we don't want to accidentally save this back to the database)
            var redacted = record.With(r =>
            {
                r.Gemini.ResponsibleOrganisation.Name = "Digital and Data Solutions, JNCC";
                r.Gemini.ResponsibleOrganisation.Email = "data@jncc.gov.uk";
                r.Gemini.MetadataPointOfContact.Name = "Digital and Data Solutions, JNCC";
                r.Gemini.MetadataPointOfContact.Email = "data@jncc.gov.uk";
            });

            return redacted;
        }

        public string UpdateWafIndexDocument(Record record, string indexDocHtml)
        {
            var doc = XDocument.Parse(indexDocHtml);
            var body = doc.Root.Element("body");

            var newLink = new XElement("a", new XAttribute("href", record.Id + ".xml"), record.Gemini.Title);
            var existingLinks = body.Elements("a").ToList();

            existingLinks.Remove();

            var newLinks = existingLinks
                .Concat(new[] { newLink })
                .GroupBy(a => a.Attribute("href").Value)
                .Select(g => g.First()); // poor man's DistinctBy

            body.Add(newLinks);

            return doc.ToString();
        }

        public static List<OnlineResource> GetOnlineResources(Record record)
        {
            var onlineResources = new List<OnlineResource>();
            if (record.Publication.Target.Hub != null && record.Publication.Target.Hub.Publishable == true &&
                record.Publication.Target.Hub.Url.IsNullOrWhiteSpace() != true)
            {
                onlineResources.Add(new OnlineResource
                {
                    Name = "JNCC ResourceHub Page",
                    Url = record.Publication.Target.Hub.Url
                });
            }
            else
            {
                onlineResources = GetOnlineResourcesFromDataResources(record);
            }

            return onlineResources;
        }

        public static List<OnlineResource> GetOnlineResourcesFromDataResources(Record record)
        {
            var resources = new List<OnlineResource>();
            if (record.Publication?.Data?.Resources?.Count > 0)
            {
                foreach (var resource in record.Publication.Data.Resources)
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

        private static bool IsFileResource(Resource resource)
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
    }
}