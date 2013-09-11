using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Catalogue.Web.Controllers.Validator
{
    public class ValidatorController : ApiController
    {
        // POST api/validation
        public ValidatorResult Post([FromBody]ValidatorInputModel input)
        {
            Uri uri;
            bool valid = Uri.TryCreate(input.Value, UriKind.Absolute, out uri);

            return new ValidatorResult
                {
                    Valid = valid,
                    Message = "Not a valid file path or URI"
                };
        }

    }
}