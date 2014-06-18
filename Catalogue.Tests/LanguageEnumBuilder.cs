using System;
using System.Globalization;
using NUnit.Framework;

namespace Catalogue.Tests
{
    internal class LanguageEnumBuilder
    {
        [Test]
        [Explicit]
        public void build()
        {
            CultureInfo[] cinfo = CultureInfo.GetCultures(CultureTypes.AllCultures & ~CultureTypes.NeutralCultures);
            RegionInfo ri = null;
            foreach (CultureInfo cul in cinfo)
            {
                try
                {
                    ri = new RegionInfo(cul.Name);
                    Console.WriteLine(cul.TwoLetterISOLanguageName + "-" + ri.TwoLetterISORegionName);
                }
                catch
                {
                }
            }
        }
    }
}