using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Catalogue.Web.Code.Account;

namespace Catalogue.Web.Account
{
    public class SecurityGroupsController
    {
        readonly IUserContext userContext;

        public SecurityGroupsController(IUserContext userContext)
        {
            this.userContext = userContext;
        }

        public string Get()
        {
            return userContext.User.Groups;
        }
    }
}