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
        public DateTime LastSuccess { get; set; }
    }
}
