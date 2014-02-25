using System;
using System.Collections.Generic;

namespace Catalogue.Utilities.Collections
{
    /// <summary>
    /// Helper class to exploit C# collection initializers.
    /// </summary>
    public class StringPairList : List<Tuple<string, string>>
    {
        public void Add(string first, string second)
        {
            Add(new Tuple<string, string>(first, second));
        }
    }
}
