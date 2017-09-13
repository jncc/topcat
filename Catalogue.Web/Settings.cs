using System.Collections.Specialized;
using System.Configuration;

namespace Catalogue.Web
{
    public interface ISettings
    {
        string Domain { get; }
        string OpenDataIaoRole { get; }
    }

    public class Settings : ISettings
    {
        public string Domain
        {
            get { return "jncc-dc08.green.jncc.gov.uk"; }
        }

        public string OpenDataIaoRole
        {
            get
            {
                var allRoles = (NameValueCollection) ConfigurationManager.GetSection("roles");
                return allRoles["OpenDataIaoRole"];
            }
        }
    }
}