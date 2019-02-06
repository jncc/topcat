using Catalogue.Data.Model;

namespace Catalogue.Web.Controllers.Records
{
    public class RecordOutputModel
    {
        public Record Record { get; set; }
        public RecordState RecordState { get; set; }
    }
}