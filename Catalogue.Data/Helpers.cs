using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalogue.Data
{
    public static class Helpers
    {
        public static string GetRecordId(Guid id)
        {
            return "records/" + id;
        }
    }
}
