using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalogue.Gemini.Roles
{
    public static class ResponsiblePartyRoles
    {
        public static List<string> Allowed = new List<string>
            {
                "Resource provider",
                "Custodian",
                "Owner",
                "User",
                "Distributor",
                "Originator",
                "Point of Contact",
                "Principle Investigator",
                "Processor",
                "Publisher",
                "Author",
            };
    }
}
