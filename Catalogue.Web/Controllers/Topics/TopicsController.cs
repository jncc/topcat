using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Catalogue.Gemini.Vocabs;

namespace Catalogue.Web.Controllers.Topics
{
    public class TopicsController : ApiController
    {
        // GET api/topics
        public IEnumerable<TopicModel> Get()
        {
            return from t in TopicCategories.Values
                   select new TopicModel { Key = t.Key, Value = t.Value };
        }
    }
}
