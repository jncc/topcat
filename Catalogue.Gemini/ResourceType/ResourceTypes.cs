using System;
using System.Collections.Generic;

namespace Catalogue.Gemini.ResourceType
{
    public static class ResourceTypes
    {
        // http://www.agi.org.uk/storage/standards/uk-gemini/GEMINI2_1_published.pdf

        public static List<string> Allowed = new List<string>
            {
                "dataset",
                "series",
                "service",
                "nonGeographicDataset",
                "publication"
            };
    }
}
