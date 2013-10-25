using System;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;

namespace Catalogue.Web.Code.Account
{
    public interface IUserContext
    {
        User User { get; }
    }

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
                // just do this once since it must be expensive
                if (user == null && environment.WindowsAuthentication && principal.Identity.IsAuthenticated)
                {
                    try
                    {
                        var domainContext = new PrincipalContext(ContextType.Domain, settings.Domain);
                        var u = UserPrincipal.FindByIdentity(domainContext, principal.Identity.Name);

                        user = new User(u.EmailAddress, u.DisplayName, u.GivenName);
                    }
                    catch (PrincipalServerDownException)
                    {
                        // swallow and proceed as guest user
                    }

                }

                user = new User("guest@example.com", "Guest User", "Guest");

                return user;
            }
        }
    }
}
