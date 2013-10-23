using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Catalogue.Gemini.Model;

namespace Catalogue.Web.Controllers.Keywords
{
    public class KeywordsController : ApiController
    {
        public List<Keyword> Get(string s)
        {
            return new List<Keyword>();
        }
    }
}