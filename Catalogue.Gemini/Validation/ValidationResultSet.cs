using System;
using System.Collections.Generic;


namespace Catalogue.Gemini.Validation
{
    public class ValidationResultSet
    {
        public string FileIdentifier { get; set; }
        public List<ValidationResult> Results { get; set; } 
    }

    public class ValidationResult
    {
        public string Validation { get; set; }
        public string Status { get; set; }
        public List<ValidationError> Errors { get; set; }
    }

    public class ValidationError
    {
        public string Location { get; set; }
        public string FailedAssertion { get; set; }
        public string Message { get; set; }
    }
}
