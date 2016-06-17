using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalogue.Data.Model
{
    public class PublicationInfo
    {
        public OpenDataPublicationInfo OpenData { get; set; }
    }

    public class OpenDataPublicationInfo
    {
        public PublicationAttempt LastAttempt { get; set; }
        public PublicationAttempt LastSuccess { get; set; }

        public List<Resource> Resources { get; set; }

        /// <summary>
        /// Don't publish this record, for the time being.
        /// </summary>
        public bool Paused { get; set; }
    }

    public class Resource
    {
        public string Path { get; set; }
    }

    public class PublicationAttempt
    {
        public DateTime DateUtc { get; set; }
        public string Message { get; set; }
    }

}
