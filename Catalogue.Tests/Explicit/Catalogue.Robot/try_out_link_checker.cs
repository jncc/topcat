using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Robot.DeadLinks;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Explicit.Catalogue.Robot
{
    public class try_out_link_checker : DatabaseTestFixture
    {
        [Test]
        public void go()
        {
            var checker = new Checker(Db, new FileLinkChecker());
            var results = checker.CheckAll();

            results.Count.Should().BeGreaterThan(100);
        }
    }
}
