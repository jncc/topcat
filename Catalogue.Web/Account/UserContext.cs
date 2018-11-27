using System;
using System.Collections.Specialized;
using System.Configuration;
using Catalogue.Web.Code;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Principal;
using Catalogue.Data.Model;

namespace Catalogue.Web.Account
{
    public interface IUserContext
    {
        User User { get; }
        bool Authenticated { get; }
    }

    /// <summary>
    /// Lazily provides (and then retains in its object lifetime) user details from Active Directory.
    /// </summary>
    public class UserContext : IUserContext
    {
        readonly IPrincipal principal;
        readonly ISettings settings;
        readonly IEnvironment environment;

        public UserContext(IPrincipal principal, ISettings settings, IEnvironment environment)
        {
            this.principal = principal;
            this.settings = settings;
            this.environment = environment;
        }

        User user;

        public User User
        {
            get
            {
                // query active directory for user details
                // just do this once since it is surely expensive
                if (user == null && environment.WindowsAuthentication && principal.Identity.IsAuthenticated)
                {
                    // may throw PrincipalServerDownException and presumably a lot more...

                    var domainContext = new PrincipalContext(ContextType.Domain, settings.Domain);
                    var u = UserPrincipal.FindByIdentity(domainContext, principal.Identity.Name);
                    var group = GroupPrincipal.FindByIdentity(domainContext, settings.OpenDataIaoRole);

                    bool inIaoGroup = group != null && group.GetMembers(true).Contains(u);
                    user = new User(u.DisplayName, u.GivenName, u.EmailAddress, inIaoGroup);
                }

                return user ?? new User("Guest User", "Guest", "guest@example.com", true);
            }
        }

        public bool Authenticated
        {
            get { return principal.Identity.IsAuthenticated; }
        }

    }

    public class TestUserContext : IUserContext
    {
        public User User { get; private set; }
        public bool Authenticated { get { return true; } }

        public TestUserContext(string displayName, string firstName, string email, bool inIaoGroup)
        {
            User = new User(displayName, firstName, email, inIaoGroup);
        }

        public TestUserContext()
        {
            User = new User("Test User", "Tester", "tester@example.com", true);
        }
    }
}
