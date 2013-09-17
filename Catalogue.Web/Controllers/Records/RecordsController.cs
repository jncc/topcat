using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.Templates;

namespace Catalogue.Web.Controllers.Records
{
    public class RecordsController : ApiController
    {
        // GET api/items/5 (get an item)
        public Record Get(int id)
        {
            return new Record
                {
                    Gemini = Library.Example(),
                };
        }
    }
}

