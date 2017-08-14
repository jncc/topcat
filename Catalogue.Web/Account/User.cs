namespace Catalogue.Web.Account
{
    public class User
    {
        public string DisplayName { get; private set; }
        public string FirstName { get; private set; }
        public string Email { get; private set; }
        public bool IsIaoUser { get; private set; }

        public User(string displayName, string firstName, string email, bool isIaoUser)
        {
            FirstName = firstName;
            DisplayName = displayName;
            Email = email;
            IsIaoUser = isIaoUser;
        }
    }
}
