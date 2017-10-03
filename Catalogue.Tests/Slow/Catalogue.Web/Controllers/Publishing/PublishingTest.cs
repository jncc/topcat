using Catalogue.Data.Model;
using Catalogue.Data.Test;
using Catalogue.Data.Write;
using Catalogue.Web.Account;
using Catalogue.Web.Controllers.Publishing;
using Moq;
using Raven.Client;

namespace Catalogue.Tests.Slow.Catalogue.Web.Controllers.Publishing
{
    class PublishingTest
    {
        protected IDocumentSession GetNewDbWithRecord(Record record)
        {
            var store = new InMemoryDatabaseHelper().Create();
            using (var db = store.OpenSession())
            {
                db.Store(record);
                db.SaveChanges();

                return db;
            }
        }

        protected OpenDataPublishingController GetTestOpenDataPublishingController(IDocumentSession db)
        {
            var testUserContext = new TestUserContext();
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(u => u.User).Returns(testUserContext.User);

            var recordService = new RecordService(db, new RecordValidator());
            var publishingService = new OpenDataPublishingService(recordService);

            return new OpenDataPublishingController(db, publishingService, userContextMock.Object);
        }
    }
}
