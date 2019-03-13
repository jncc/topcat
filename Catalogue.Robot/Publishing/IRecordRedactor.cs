using Catalogue.Data.Model;

namespace Catalogue.Robot.Publishing
{
    public interface IRecordRedactor
    {
        Record RedactRecord(Record record);
    }
}
