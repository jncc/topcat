using System.Web.Http;
using Catalogue.Data.Model;

namespace Catalogue.Web.Account
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
