using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalogue.Data.Model
{
    public enum Security
    {
        // new
        Official = 0,
        OfficialSensitive = 1,
        Secret = 2,

        // old - remove!
        Open, 
        Restricted,
        Classified,

    }
}
