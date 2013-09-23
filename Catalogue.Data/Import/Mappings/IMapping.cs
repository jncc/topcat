using CsvHelper.Configuration;

namespace Catalogue.Data.Import.Mappings
{
    public interface IMapping
    {
        void Apply(CsvConfiguration config);
    }
}
