using Catalogue.Web.Controllers.Marking;
using NUnit.Framework;

namespace Catalogue.Tests.Slow.Catalogue.Web.Controllers.Marking
{
    class marking_specs : DatabaseTestFixture
    {
        [Test]
        public void marking_security_group_test()
        {
            var markingController = new OpenDataMarkingController(Db);
        }
    }
}
