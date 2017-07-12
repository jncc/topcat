using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Catalogue.Web.Code.Account
{
    public class User
    {
        public string DisplayName { get; }
        public string FirstName { get; }
        public string Email { get; }
        public string Groups { get; }

        public User(string displayName, string firstName, string email, string groups)
        {
            FirstName = firstName;
            DisplayName = displayName;
            Email = email;
            Groups = groups;
        }
    }
}
