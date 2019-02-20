using System.Collections.Specialized;
using System.Configuration;

namespace Catalogue.Web
{
    public interface ISettings
    {
        string Domain { get; }
        string PublishingIaoRole { get; }
    }

    public class Settings : ISettings
    {
        public string Domain
        {
            get { return "jncc-dc12.green.jncc.gov.uk"; }
        }

        public string PublishingIaoRole
        {
            get
            {
                var allRoles = (NameValueCollection) ConfigurationManager.GetSection("roles");
                return allRoles["OpenDataIaoRole"];
            }
        }
    }
}