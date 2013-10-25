using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Catalogue.Web.Code.Account;

namespace Catalogue.Web.Controllers.Account
{
    public class AccountController : ApiController
    {
        readonly IUserContext userContext;

        public AccountController(IUserContext userContext)
        {
            this.userContext = userContext;
        }

        public User Get()
        {
            return userContext.User;
        }
    }
}
