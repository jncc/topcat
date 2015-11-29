using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalogue.Robot.Publishing.DataGovUk
{
    public class DataGovUkPublisherConfig
    {
        public string HttpRootUrl { get; set; }
        public string FtpRootUrl { get; set; }
        public string FtpUsername { get; set; }
        public string FtpPassword { get; set; }
    }
}
