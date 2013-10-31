using System;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;
using System.Web;

namespace Catalogue.Web.Code.Account
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

                    user = new User(u.EmailAddress, u.DisplayName, u.GivenName);
                }

                return user ?? new User("Guest User", "Guest", "guest@example.com");
            }
        }

        public bool Authenticated
        {
            get { return principal.Identity.IsAuthenticated; }
        }
    }
}
