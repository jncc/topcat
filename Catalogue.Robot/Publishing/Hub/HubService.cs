using System.Collections.Generic;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Clone;
using log4net;
using Raven.Client.Documents.Session;

namespace Catalogue.Robot.Publishing.Hub
{
    public class HubService : IHubService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HubService));

        private readonly HubApiHelper apiHelper;
        private readonly QueueClient queueClient;
        private readonly HubMessageConverter hubMessageConverter;
        private readonly IRecordRedactor recordRedactor;

        public HubService(IRecordRedactor recordRedactor)
        {
            this.recordRedactor = recordRedactor;
            this.apiHelper = new HubApiHelper();
            this.queueClient = new QueueClient();
            this.hubMessageConverter = new HubMessageConverter(new FileHelper());
        }

        public void Save(Record record)
        {
            var redactedRecord = recordRedactor.RedactRecord(record);

            Logger.Info("Saving record as an asset to the Hub database");
            apiHelper.Upsert(redactedRecord);
        }

        public void Index(Record record)
        {
            Logger.Info("Attempting to add the record to the queue for search indexing");

            var redactedRecord = recordRedactor.RedactRecord(record);

            var message = hubMessageConverter.ConvertRecordToQueueMessage(redactedRecord);

            Logger.Debug($"Message to send: {message}");

            //queueClient.Send(message);

            Logger.Info("Message successfully added to queue");
        }
    }
}
