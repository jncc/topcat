using CsvHelper.Configuration;

namespace Catalogue.Data.Import.Formats
{
    public interface IFormat
    {
        void Configure(CsvConfiguration config);
    }
}
