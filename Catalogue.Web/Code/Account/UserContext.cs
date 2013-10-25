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
                        var userPrincipal = UserPrincipal.FindByIdentity(domainContext, principal.Identity.Name);

                        user = new User(userPrincipal.EmailAddress, userPrincipal.DisplayName, userPrincipal.GivenName);
                    }
                    catch (PrincipalServerDownException)
                    {
                        // swallow and proceed as guest user
                    }
                }

                return user ?? new User("guest@example.com", "Guest User", "Guest");
            }
        }
    }
}
