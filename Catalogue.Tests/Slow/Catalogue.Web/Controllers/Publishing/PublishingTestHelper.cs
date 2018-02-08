using Catalogue.Data.Model;
using Catalogue.Data.Test;
using Catalogue.Data.Write;
using Catalogue.Web.Account;
using Catalogue.Web.Controllers.Publishing;
using Moq;
using Raven.Client;

namespace Catalogue.Tests.Slow.Catalogue.Web.Controllers.Publishing
{
    public static class PublishingTestHelper
    {
        public static IDocumentSession GetNewDbSessionWithThisRecordAdded(Record record)
        {
            var store = new InMemoryDatabaseHelper().Create();
            using (var db = store.OpenSession())
            {
                db.Store(record);
                db.SaveChanges();

                return db;
            }
        }

        public static OpenDataPublishingController GetTestOpenDataPublishingController(IDocumentSession db)
        {
            var testUserContext = new TestUserContext();
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(u => u.User).Returns(testUserContext.User);

            var publishingService = new OpenDataPublishingRecordService(db, new RecordValidator());

            return new OpenDataPublishingController(db, publishingService, userContextMock.Object);
        }
    }
}
