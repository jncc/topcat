using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalogue.Utilities.Collections
{
    public static class DictionaryExtensions
    {
        public static bool IsStructurallyEqualTo<K, V>(this Dictionary<K, V> source, Dictionary<K, V> other)
        {
            // http://www.dotnetperls.com/dictionary-equals

            bool equal = false;

            if (source.Count == other.Count) // Require equal count.
            {
                equal = true;
                foreach (var pair in source)
                {
                    V value;
                    if (other.TryGetValue(pair.Key, out value))
                    {
                        // Require value be equal.
                        if (!value.Equals(pair.Value))
                        {
                            equal = false;
                            break;
                        }
                    }
                    else
                    {
                        // Require key be present.
                        equal = false;
                        break;
                    }
                }
            }

            return equal;
        }
    }
}
