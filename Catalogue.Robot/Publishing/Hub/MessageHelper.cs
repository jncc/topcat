using Catalogue.Data;
using Catalogue.Data.Model;
using Newtonsoft.Json;

namespace Catalogue.Robot.Publishing.Hub
{
    public class MessageHelper
    {
        public string ConvertRecordToQueueMessage(Record record)
        {
            string message = null;

            var simpleMessage = new
            {
                verb = "upsert",
                index = "topcatdev",
                document = new
                {
                    id = Helpers.RemoveCollection(record.Id),
                    site = "datahub", // as opposed to datahub|sac|mhc
                    title = record.Gemini.Title,
                    content = record.Gemini.Abstract,
                    url = "http://example.com/pages/123456789", // the URL of the page, for clicking through
                    keywords = record.Gemini.Keywords,
                    published_date = record.Gemini.DatasetReferenceDate
                }
            };

            message = JsonConvert.SerializeObject(simpleMessage, Formatting.None);

            return message;
        }
    }
}
