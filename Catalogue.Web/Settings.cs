using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Catalogue.Web.Code
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