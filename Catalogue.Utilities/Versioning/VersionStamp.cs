using System.Linq;
using System.Reflection;

namespace Catalogue.Utilities.Versioning
{
    public class VersionStamp
    {
        public static string Stamp = GetVersionStamp();

        static string GetVersionStamp()
        {
            return Assembly.GetExecutingAssembly()
                       .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                       .Cast<AssemblyInformationalVersionAttribute>()
                       .Select(a => a.InformationalVersion)
                       .SingleOrDefault() ?? "0.0.0.0";
        }
    }
}
