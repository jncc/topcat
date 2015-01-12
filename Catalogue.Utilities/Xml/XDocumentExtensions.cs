using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Catalogue.Utilities.Xml
{
    public static class XDocumentExtensions
    {
        /// <summary>
        /// Avoids http://stackoverflow.com/questions/1228976/xdocument-tostring-drops-xml-encoding-tag
        /// </summary>
        public static string ToStringWithDeclaration(this XDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException("doc");
            }
            StringBuilder builder = new StringBuilder();
            using (TextWriter writer = new StringWriter(builder))
            {
                doc.Save(writer);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Returns the document encoded with UTF-8. 
        /// </summary>
        public static string ToUtf8DocumentString(this XDocument doc)
        {
            var writer = new Utf8StringWriter();
            doc.Declaration = new XDeclaration("1.0", "utf-8", null);
            doc.Save(writer, SaveOptions.None);

            return writer.ToString();
        }

        private class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }
    }
}
