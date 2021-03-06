﻿using Catalogue.Gemini.Encoding;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.Templates;
using Catalogue.Gemini.Validation;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace Catalogue.Tests.Slow.Catalogue.Gemini.Validation
{
    /// <summary>
    ///     This is an end-to-end style test that exercises several bits of the system
    ///     and depends on an external web service.
    /// </summary>
    public class when_validating_the_example_document
    {
        [Test, Explicit]
        public void should_be_valid_gemini()
        {
            // without this we get an error message 417
            // http://stackoverflow.com/questions/566437/http-post-returns-the-error-417-expectation-failed-c
            ServicePointManager.Expect100Continue = false;

            // start with the example document
            Metadata metadata = Library.Example();

            // ...encode it into xml
            XDocument doc = new XmlEncoder().Create("b97aac01-5e5d-4209-b626-514e40245bc1", metadata, new List<OnlineResource>());

            // ...validate it with the CEH validator
            ValidationResultSet result = new Validator().Validate(doc);

            // IGNORE MEDIN VALIDATION ERRORS
            result.Results.Single(r => r.Validation.StartsWith("GEMINI2"))
                .Valid
                .Should().BeTrue();
        }
    }
}