using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalogue.Data.Model
{
    public abstract class Publisher
    {
        public abstract void Publish();
        public List<PublicationAttempt> Attempts { get; set; } 
    }

    public class PublicationAttempt
    {
        public DateTime DateUtc { get; set; }
        public bool Successful { get; set; }
        public string Message { get; set; }
    }
}
