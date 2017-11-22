using Catalogue.Data.Model;
using Raven.Client.Listeners;
using Raven.Json.Linq;

namespace Catalogue.Data
{
    public class InternalContactToManagerConverter : IDocumentConversionListener
    {
        public void BeforeConversionToDocument(string key, object entity, RavenJObject metadata) { }

        public void AfterConversionToDocument(string key, object entity, RavenJObject document, RavenJObject metadata)
        {
            if (entity is Record == false)
                return;

            document.Remove("InternalContact");
        }

        public void BeforeConversionToEntity(string key, RavenJObject document, RavenJObject metadata) { }

        public void AfterConversionToEntity(string key, RavenJObject document, RavenJObject metadata, object entity)
        {
            if (!(entity is Record))
                return;

            var r = (Record)entity;

            if (r.Manager == null)
            {
                var internalContact = document.Value<RavenJObject>("InternalContact");
                if (internalContact != null)
                {
                    r.Manager = new UserInfo();

                    var displayName = internalContact.Value<string>("DisplayName");
                    if (displayName != null)
                        r.Manager.DisplayName = displayName;

                    var email = internalContact.Value<string>("Email");
                    if (email != null)
                        r.Manager.Email = email;
                }
            }
        }
    }
}