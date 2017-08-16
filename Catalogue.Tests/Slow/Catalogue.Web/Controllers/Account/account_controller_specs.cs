using Catalogue.Web.Account;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Catalogue.Tests.Slow.Catalogue.Web.Controllers.Account
{
    class account_controller_specs
    {
        [Test]
        public void account_in_iao_group()
        {
            var testUserContext = new TestUserContext();
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(u => u.User).Returns(testUserContext.User);

            var accountController = new AccountController(userContextMock.Object);

            var account = accountController.Get();
            account.IsIaoUser.Should().BeTrue();
        }

        [Test]
        public void account_not_in_iao_group()
        {
            var testUserContext = new TestUserContext("Test User", "Tester", "tester@example.com", false);
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(u => u.User).Returns(testUserContext.User);

            var accountController = new AccountController(userContextMock.Object);

            var account = accountController.Get();
            account.IsIaoUser.Should().BeFalse();
        }
    }
}
