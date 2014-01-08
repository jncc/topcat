using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Catalogue.Gemini.DataFormats;

namespace Catalogue.Web.Controllers.Formats
{
    public class FormatsController : ApiController
    {
        public DataFormatGroupCollection Get()
        {
            return DataFormats.Known;
        }
    }
}