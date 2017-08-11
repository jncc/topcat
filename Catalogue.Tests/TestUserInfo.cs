using Catalogue.Data.Model;

namespace Catalogue.Tests
{
    public static class TestUserInfo
    {
        public static UserInfo TestUser => new UserInfo
        {
            DisplayName = "Test User",
            Email = "test.user@jncc.gov.uk"
        };
    }
}
