using System.Collections.Generic;
using NUnit.Framework;

namespace Catalogue.Robot.Publishing.OpenData
{
    public class OpenDataPublisherConfig
    {
        public string HttpRootUrl { get; set; }
        public string FtpRootUrl { get; set; }
        public string FtpUsername { get; set; }
        public string FtpPassword { get; set; }
    }
}
