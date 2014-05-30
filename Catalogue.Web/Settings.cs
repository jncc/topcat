using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Catalogue.Web.Code
{
    public interface ISettings
    {
        string Domain { get; }
    }

    public class Settings : ISettings
    {
        public string Domain
        {
            get { return "jncc-dc08.green.jncc.gov.uk"; }
        }
    }
}