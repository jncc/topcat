using System.Collections.Generic;

namespace Catalogue.Gemini.Model
{
    public class Vocabulary
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PublicationDate { get; set; }
     
        public List<string> Values { get; set; }
    }
}
