using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Catalogue.Gemini.Vocabs;

namespace Catalogue.Web.Controllers.Topics
{
    public class TopicsController : ApiController
    {
        // GET api/topics
        public IEnumerable<TopicCategory> Get()
        {
            return from t in TopicCategories.Values where t.Relevant select t;
        }
    }
}
