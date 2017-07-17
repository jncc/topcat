using System;
using Catalogue.Data.Model;
using Catalogue.Data.Test;
using Catalogue.Data.Write;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using Catalogue.Web.Controllers.Marking;
using NUnit.Framework;

namespace Catalogue.Tests.Slow.Catalogue.Web.Controllers.Marking
{
    class marking_specs
    {
        [Test]
        public void marking_security_group_test()
        {
            var record = new Record().With(r =>
            {
                r.Id = new Guid("f34de2d3-17af-47e2-8deb-a16b67c76b06");
                r.Path = @"X:\path\to\marking\test";
                r.Gemini = Library.Blank().With(m =>
                {
                    m.Title = "Open data marking test";
                });
            });

            var store = new InMemoryDatabaseHelper().Create();
            using (var db = store.OpenSession())
            {
                db.Store(record);
                db.SaveChanges();

                var markingController = new OpenDataMarkingController(db);
                markingController.MarkAsOpenData(record.Id);

                var markedRecord = db.Load<Record>(record.Id);


            }
        }
    }
}
