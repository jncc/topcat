using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Catalogue.Web.Controllers.Validator
{
    public class ValidatorResult
    {
        public bool Valid { get; set; }
        public string Message { get; set; }
    }
}