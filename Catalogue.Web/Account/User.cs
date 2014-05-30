using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Catalogue.Web.Code.Account
{
    public class User
    {
        public string DisplayName { get; private set; }
        public string FirstName { get; private set; }
        public string Email { get; private set; }

        public User(string displayName, string firstName, string email)
        {
            FirstName = firstName;
            DisplayName = displayName;
            Email = email;
        }
    }
}