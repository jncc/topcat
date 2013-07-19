using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Catalogue.Gemini.Encoding
{
    public static class XPathExtensions
    {
        /// <summary>
        /// A more convenient XPathSelectElement which works with the standard ISO namespaces.
        /// </summary>
        public static XElement XPath(this XDocument doc, string xpath)
        {
            var r = doc.CreateReader();
            var m = new XmlNamespaceManager(r.NameTable);

            // add the standard namespaces for iso encoding

            m.AddNamespace("gmd", XmlEncoder.gmd.NamespaceName);
            m.AddNamespace("gco", XmlEncoder.gco.NamespaceName);
            m.AddNamespace("srv", XmlEncoder.srv.NamespaceName);
            m.AddNamespace("gml", XmlEncoder.gml.NamespaceName);
            m.AddNamespace("xlink", XmlEncoder.xlink.NamespaceName);

            return doc.XPathSelectElement(xpath, m);
        }
    }
}
