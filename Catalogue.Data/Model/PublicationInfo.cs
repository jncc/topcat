using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalogue.Data.Model
{
    public class PublicationInfo
    {
        public DataGovUkPublicationInfo DataGovUk { get; set; }
    }

    public class DataGovUkPublicationInfo
    {
        public List<PublicationAttempt> Attempts { get; set; }         
    }

    public class PublicationAttempt
    {
        public DateTime DateUtc { get; set; }
        public bool Successful { get; set; }
        public string Message { get; set; }
    }

}
