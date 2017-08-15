﻿using System;
using System.Collections.Specialized;
using System.Configuration;
using Catalogue.Utilities.Text;
using Catalogue.Web.Code;
using System.DirectoryServices.AccountManagement;
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

                    var allRoles = (NameValueCollection)ConfigurationManager.GetSection("roles");
                    var group = GroupPrincipal.FindByIdentity(domainContext, allRoles["OpenDataSiroRole"]);

                    if (u != null && group != null)
                    {
                        var inIaoGroup = u.IsMemberOf(group);
                        if (inIaoGroup)
                        {
                            user = new User(u.DisplayName, u.GivenName, u.EmailAddress, inIaoGroup);
                        }
                        else
                        {
                            throw new Exception(u.DisplayName+" not in "+group.Name+", security group? "+group.IsSecurityGroup);
                        }
                    }
                    else
                    {
                        throw new Exception("Error cannot check IAO group");
                    }
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
