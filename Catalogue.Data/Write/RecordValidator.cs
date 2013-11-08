using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using Catalogue.Utilities.Text;

namespace Catalogue.Data.Write
{
    public interface IRecordValidator
    {
        RecordValidationResult Validate(Record record);
    }

    public class RecordValidator : IRecordValidator
    {
        public RecordValidationResult Validate(Record record)
        {
            if (record.Gemini.ResourceLocator.IsBlank())
                return new RecordValidationResult { Message = "ResourceLocator must not be blank." };

            return new RecordValidationResult { Success = true };
        }
    }


    public class RecordValidationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
