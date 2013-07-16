using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Catalogue.Web.Controllers.Items
{
    public class ItemsController : ApiController
    {
        // GET api/items
        public IEnumerable<string> Get()
        {
            return new [] { "value1", "value2" };
        }

        // GET api/items/5 (get an item)
        public string Get(int id)
        {
            return "value";
        }

        // POST api/items (add a new item)
        public void Post([FromBody]string value)
        {
        }

        // PUT api/items/5 (update/replace an item)
        public void Put(int id, [FromBody]string value)
        {
        }
    }
}