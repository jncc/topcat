using System;
using CsvHelper.Configuration;

namespace Catalogue.Import.Formats
{
    public interface IFormat
    {
        void Configure(CsvConfiguration config);
    }
}
