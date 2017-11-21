﻿using Catalogue.Data.Model;
using Raven.Client.Listeners;
using Raven.Json.Linq;

namespace Catalogue.Data
{
    public class InternalContactToManagerConverter : IDocumentConversionListener
    {
        public void BeforeConversionToDocument(string key, object entity, RavenJObject metadata) { }
        public void AfterConversionToDocument(string key, object entity, RavenJObject document, RavenJObject metadata) { }
        public void BeforeConversionToEntity(string key, RavenJObject document, RavenJObject metadata) { }

        public void AfterConversionToEntity(string key, RavenJObject document, RavenJObject metadata, object entity)
        {
            if (!(entity is Record))
                return;

            var r = (Record)entity;

            if (r.Manager == null)
            {
                r.Manager = document.Value<UserInfo>("InternalContact");
            }
        }
    }
}