using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using FluentAssertions;
using NUnit.Framework;

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


    class validator_controller_tests
    {
        [Test]
        public void should_validate_location_with_valid_uri()
        {
            var c = new ValidatorController();
            var result = c.Post(new ValidatorInputModel { Value = "http://example.com/nice/url" });

            result.Valid.Should().BeTrue();
        }

        [Test]
        public void should_not_validate_location_with_invalid_uri()
        {
            var c = new ValidatorController();
            var result = c.Post(new ValidatorInputModel { Value = "not/a/url" });

            result.Valid.Should().BeFalse();
        }
    }
}