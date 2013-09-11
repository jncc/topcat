using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Web.Controllers.Validator;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Fast.Catalogue.Web.Controllers.Validator
{
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
