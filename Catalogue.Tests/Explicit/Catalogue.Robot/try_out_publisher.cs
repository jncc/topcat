using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Robot.Publishing.DataGovUk;
using NUnit.Framework;

namespace Catalogue.Tests.Explicit.Catalogue.Robot
{
    class try_out_publisher : DatabaseTestFixture
    {
        [Test, Explicit]
        public void do_it()
        {
            var publisher = new DataGovUkPublisher(Db);
            publisher.Publish();
        }
    }
}
