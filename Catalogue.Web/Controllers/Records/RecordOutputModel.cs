using Catalogue.Data.Model;
using Catalogue.Data.Publishing;

namespace Catalogue.Web.Controllers.Records
{
    public class RecordOutputModel
    {
        public Record Record { get; set; }
        public RecordState RecordState { get; set; }
        public PublishingPolicyResult PublishingPolicy { get; set; }
    }
}