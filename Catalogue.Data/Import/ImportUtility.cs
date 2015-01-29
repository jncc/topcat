using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Utilities.Text;

namespace Catalogue.Data.Import
{
    public class ImportUtility
    {
        public static string ParseDate(string d)
        {
            return d.IsNotBlank() ? DateTime.Parse(d).ToString("yyyy-MM-dd") : String.Empty;
        }
    }
}
