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
    public class try_out_link_checker : SeededDbTest
    {
        [Test, Explicit]
        public void go()
        {
            var checker = new Checker(Db, new FileLinkChecker());
            var results = checker.CheckAll();

            results.Count.Should().BeGreaterThan(100);
        }

        [Test, Explicit]
        public void yep()
        {
            var checker = new Checker(Db, new FileLinkChecker());
            var result = checker.CheckLink("records/"+Guid.NewGuid(), @"C:\work");

            result.Status.Should().Be(LinkCheckStatus.Ok);
        }
    }
}
