using Catalogue.Data.Model;
using Raven.Client.Listeners;
using Raven.Json.Linq;

namespace Catalogue.Data
{
    public class UserToUserInfoConverter : IDocumentConversionListener
    {
        public void BeforeConversionToDocument(string key, object entity, RavenJObject metadata){}
        public void AfterConversionToDocument(string key, object entity, RavenJObject document, RavenJObject metadata) {}
        public void BeforeConversionToEntity(string key, RavenJObject document, RavenJObject metadata) {}

        public void AfterConversionToEntity(string key, RavenJObject document, RavenJObject metadata, object entity)
        {
            if (entity is Record == false)
                return;

            var r = (Record) entity;
            var footer = document.Value<RavenJObject>("Footer");

            if (r.Footer.CreatedByUser == null)
            {
                r.Footer.CreatedByUser = new UserInfo
                {
                    DisplayName = footer.Value<string>("CreatedBy"),
                    Email = "data@jncc.gov.uk"
                };
            }

            if (r.Footer.ModifiedByUser == null)
            {
                r.Footer.ModifiedByUser = new UserInfo
                {
                    DisplayName = footer.Value<string>("ModifiedBy"),
                    Email = "data@jncc.gov.uk"
                };
            }
        }
    }
}