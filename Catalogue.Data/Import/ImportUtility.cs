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
        /// <summary>
        /// 
        /// </summary>
        public static DateTime ParseSpreadsheetDate(string str)
        {
            try
            {
                return str.IsNotBlank() ? Convert.ToDateTime(str) : DateTime.MinValue;
            }
            catch (System.FormatException)
            {
                try
                {
                    return DateTime.ParseExact(str, "yyyy-dd-MM", CultureInfo.InvariantCulture);
                }
                catch (System.FormatException)
                {
                    try
                    {
                        // its probaby just a year
                        return DateTime.ParseExact(str + "-01-01", "yyyy-dd-MM", CultureInfo.InvariantCulture);
                    }
                    catch (System.FormatException)
                    {
                        throw new Exception("Conversion failed for date :" + str);
                    }
                }
            }
        }
    }
}
